using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RateLimiting
{
    /// <summary>
    /// Configuration options for the rate limiting middleware.
    /// </summary>
    public class RateLimitOptions
    {
        /// <summary>
        /// The maximum number of requests allowed within the window. Default is 10.
        /// </summary>
        public int RequestsPerWindow { get; set; } = 10;

        /// <summary>
        /// The time window for the sliding rate limit. Default is 60 seconds.
        /// </summary>
        public TimeSpan Window { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>
        /// The header key used to identify the client. 
        /// If absent, the client's IP address is used. Default is "X-Client-Id".
        /// </summary>
        public string ClientIdHeader { get; set; } = "X-Client-Id";
    }

    /// <summary>
    /// Middleware that limits the number of requests per client using a sliding window algorithm.
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimitOptions _options;

        // Thread-safe dictionary to store client request timestamps.
        private readonly ConcurrentDictionary<string, ClientRequestHistory> _clientHistory = new();

        public RateLimitMiddleware(RequestDelegate next, IOptions<RateLimitOptions> options)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var clientId = GetClientId(context);
            var now = DateTime.UtcNow;

            // Get or add the client's request history
            var history = _clientHistory.GetOrAdd(clientId, _ => new ClientRequestHistory());

            // Clean up expired timestamps and count recent requests
            int currentCount;
            lock (history) // Lock per client history to ensure thread safety
            {
                // Remove timestamps older than the window
                while (history.Timestamps.TryPeek(out var oldestTimestamp) && 
                       now - oldestTimestamp > _options.Window)
                {
                    history.Timestamps.TryDequeue(out _);
                }

                currentCount = history.Timestamps.Count;

                // Check if limit is exceeded
                if (currentCount >= _options.RequestsPerWindow)
                {
                    // Calculate retry-after based on the oldest request in the current window
                    var oldestValid = history.Timestamps.Peek();
                    var retryAfter = _options.Window - (now - oldestValid);
                    var retryAfterSeconds = (int)Math.Ceiling(retryAfter.TotalSeconds);

                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();

                    // Short-circuit the pipeline
                    return;
                }

                // Add current request timestamp
                history.Timestamps.Enqueue(now);
                currentCount++; // Increment for the 'Remaining' header calculation
            }

            // Calculate remaining requests
            int remaining = Math.Max(0, _options.RequestsPerWindow - currentCount);
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();

            // Continue down the pipeline
            await _next(context);
        }

        private string GetClientId(HttpContext context)
        {
            // 1. Try to get from configured header
            if (!string.IsNullOrEmpty(_options.ClientIdHeader) && 
                context.Request.Headers.TryGetValue(_options.ClientIdHeader, out var headerValue) &&
                !string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue.ToString();
            }

            // 2. Fallback to IP address
            var ip = context.Connection.RemoteIpAddress?.ToString();
            return ip ?? "unknown";
        }

        // Helper class to store timestamps for a specific client
        private class ClientRequestHistory
        {
            public ConcurrentQueue<DateTime> Timestamps { get; } = new ConcurrentQueue<DateTime>();
        }
    }

    /// <summary>
    /// Extension methods for registering the rate limiting middleware.
    /// </summary>
    public static class RateLimitExtensions
    {
        /// <summary>
        /// Adds rate limiting services to the DI container.
        /// </summary>
        public static IServiceCollection AddRateLimiting(this IServiceCollection services, Action<RateLimitOptions>? configure = null)
        {
            services.Configure(configure ?? (_ => { }));
            return services;
        }

        /// <summary>
        /// Adds the rate limiting middleware to the application pipeline.
        /// </summary>
        public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RateLimitMiddleware>();
        }
    }
}