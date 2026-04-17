using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IdempotencyMiddleware
{
    // Options configuration for the middleware
    public class IdempotencyOptions
    {
        public string HeaderName { get; set; } = "Idempotency-Key";
        public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromHours(24);
    }

    // Record representing the cached response
    public record IdempotencyRecord(
        int StatusCode,
        string ContentType,
        byte[] Body,
        DateTime CreatedAt
    );

    // Interface for storing and retrieving idempotency records
    public interface IIdempotencyStore
    {
        Task<IdempotencyRecord?> GetAsync(string key, CancellationToken ct);
        Task SetAsync(string key, IdempotencyRecord record, TimeSpan expiry, CancellationToken ct);
    }

    // In-memory implementation of the store (thread-safe)
    public class InMemoryIdempotencyStore : IIdempotencyStore
    {
        private readonly ConcurrentDictionary<string, (IdempotencyRecord Record, DateTime Expiry)> _cache 
            = new ConcurrentDictionary<string, (IdempotencyRecord Record, DateTime Expiry)>();

        public Task<IdempotencyRecord?> GetAsync(string key, CancellationToken ct)
        {
            Cleanup();

            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.Expiry > DateTime.UtcNow)
                {
                    return Task.FromResult<IdempotencyRecord?>(entry.Record);
                }
                // Expired, remove it
                _cache.TryRemove(key, out _);
            }

            return Task.FromResult<IdempotencyRecord?>(null);
        }

        public Task SetAsync(string key, IdempotencyRecord record, TimeSpan expiry, CancellationToken ct)
        {
            var expiryDate = DateTime.UtcNow.Add(expiry);
            _cache[key] = (record, expiryDate);
            return Task.CompletedTask;
        }

        private void Cleanup()
        {
            // Simple cleanup strategy: check for expired items on interaction.
            // In a high-throughput production system, this should be a background service.
            foreach (var kvp in _cache)
            {
                if (kvp.Value.Expiry <= DateTime.UtcNow)
                {
                    _cache.TryRemove(kvp.Key, out _);
                }
            }
        }
    }

    // The Middleware logic
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IdempotencyOptions _options;
        private readonly IIdempotencyStore _store;

        public IdempotencyMiddleware(
            RequestDelegate next, 
            IdempotencyOptions options, 
            IIdempotencyStore store)
        {
            _next = next;
            _options = options;
            _store = store;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1. Only apply to POST requests
            if (!HttpMethods.IsPost(context.Request.Method))
            {
                await _next(context);
                return;
            }

            // 2. Check for Header
            if (!context.Request.Headers.TryGetValue(_options.HeaderName, out var keyValues) || string.IsNullOrEmpty(keyValues))
            {
                await _next(context);
                return;
            }

            string idempotencyKey = keyValues.ToString();

            // 3. Check Store for existing record
            var existingRecord = await _store.GetAsync(idempotencyKey, context.RequestAborted);

            if (existingRecord != null)
            {
                // Replay cached response
                context.Response.StatusCode = existingRecord.StatusCode;
                context.Response.ContentType = existingRecord.ContentType;

                if (existingRecord.Body.Length > 0)
                {
                    await context.Response.Body.WriteAsync(existingRecord.Body, 0, existingRecord.Body.Length, context.RequestAborted);
                }

                return;
            }

            // 4. Execute request and capture response
            // We need to intercept the response stream to cache it.
            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                await _next(context);
            }
            finally
            {
                // Restore original stream
                context.Response.Body = originalBodyStream;
            }

            // 5. Cache the response
            // Only cache successful responses? The prompt implies caching the result regardless, 
            // but typically 5xx errors shouldn't be cached. 
            // For this vertical slice, we cache the result as-is per standard idempotency definitions.

            var record = new IdempotencyRecord(
                context.Response.StatusCode,
                context.Response.ContentType ?? "application/octet-stream",
                memoryStream.ToArray(),
                DateTime.UtcNow
            );

            await _store.SetAsync(idempotencyKey, record, _options.CacheExpiry, context.RequestAborted);

            // 6. Write response to client
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream, context.RequestAborted);
        }
    }

    // Extension methods for registration
    public static class IdempotencyExtensions
    {
        public static IServiceCollection AddIdempotency(this IServiceCollection services, Action<IdempotencyOptions>? configure = null)
        {
            var options = new IdempotencyOptions();
            configure?.Invoke(options);

            services.AddSingleton(options);
            services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

            return services;
        }

        public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
        {
            return app.UseMiddleware<IdempotencyMiddleware>();
        }
    }
}