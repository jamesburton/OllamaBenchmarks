using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace VerticalSliceAudit;

// --- Interfaces ---

/// <summary>
/// Provides context information scoped to the current HTTP request.
/// </summary>
public interface IRequestContext
{
    Guid RequestId { get; }
    string? UserId { get; }
    DateTime StartedAt { get; }
}

/// <summary>
/// Provides audit logging capabilities.
/// </summary>
public interface IAuditService
{
    Task LogAsync(string action, string details, CancellationToken ct);
    Task<IReadOnlyList<AuditEntry>> GetLogsAsync(CancellationToken ct);
}

// --- Models ---

/// <summary>
/// Represents a single audit log entry.
/// </summary>
public record AuditEntry(Guid RequestId, string? UserId, string Action, string Details, DateTime At);

// --- Implementation ---

/// <summary>
/// Implementation of <see cref="IRequestContext"/> populated from HTTP headers.
/// </summary>
public class HttpRequestContext : IRequestContext
{
    public Guid RequestId { get; }
    public string? UserId { get; }
    public DateTime StartedAt { get; }

    public HttpRequestContext(Guid requestId, string? userId, DateTime startedAt)
    {
        RequestId = requestId;
        UserId = userId;
        StartedAt = startedAt;
    }
}

/// <summary>
/// Middleware that initializes the <see cref="IRequestContext"/> for the current request.
/// </summary>
public class RequestContextMiddleware
{
    private readonly RequestDelegate _next;

    public RequestContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        // 1. Extract RequestId
        Guid requestId;
        if (httpContext.Request.Headers.TryGetValue("X-Request-Id", out var requestIdValues) 
            && requestIdValues.Count > 0 
            && Guid.TryParse(requestIdValues[0], out var parsedGuid))
        {
            requestId = parsedGuid;
        }
        else
        {
            requestId = Guid.NewGuid();
        }

        // 2. Extract UserId
        string? userId = null;
        if (httpContext.Request.Headers.TryGetValue("X-User-Id", out var userIdValues) && userIdValues.Count > 0)
        {
            userId = userIdValues[0];
        }

        // 3. Create context instance
        var requestContext = new HttpRequestContext(requestId, userId, DateTime.UtcNow);

        // 4. Register in DI scope (replace existing if any)
        httpContext.RequestServices.GetRequiredService<HttpRequestContextAccessor>().Context = requestContext;

        await _next(httpContext);
    }
}

/// <summary>
/// Internal accessor class to allow the Middleware to set the context 
/// and the Service to retrieve it, resolving the DI lifetime mismatch limitations.
/// </summary>
internal class HttpRequestContextAccessor
{
    public IRequestContext? Context { get; set; }
}

/// <summary>
/// Implementation of <see cref="IAuditService"/> that utilizes the request context.
/// </summary>
public class AuditService : IAuditService
{
    // In-memory storage for demonstration purposes
    private static readonly ConcurrentBag<AuditEntry> _auditStore = new();
    private readonly IRequestContext _requestContext;

    public AuditService(IRequestContext requestContext)
    {
        _requestContext = requestContext;
    }

    public Task LogAsync(string action, string details, CancellationToken ct)
    {
        var entry = new AuditEntry(
            _requestContext.RequestId,
            _requestContext.UserId,
            action,
            details,
            DateTime.UtcNow
        );

        _auditStore.Add(entry);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditEntry>> GetLogsAsync(CancellationToken ct)
    {
        return Task.FromResult<IReadOnlyList<AuditEntry>>(_auditStore.ToArray());
    }
}

// --- DI Registration ---

/// <summary>
/// Extension methods for configuring audit services.
/// </summary>
public static class AuditServiceExtensions
{
    public static IServiceCollection AddAuditServices(this IServiceCollection services)
    {
        // Register the internal accessor as scoped so it persists for the duration of the request.
        services.AddScoped<HttpRequestContextAccessor>();

        // Register IRequestContext to resolve from the accessor.
        services.AddScoped<IRequestContext>(sp => 
            sp.GetRequiredService<HttpRequestContextAccessor>().Context 
            ?? throw new InvalidOperationException("RequestContextMiddleware has not initialized the context."));

        // Register the audit service
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }

    public static IApplicationBuilder UseRequestContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestContextMiddleware>();
    }
}