using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }

    public static IApplicationBuilder UseGlobalException(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }

    public static IApplicationBuilder UseRequestTiming(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestTimingMiddleware>();
    }
}

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private const string CorrelationIdItemKey = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId;

        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationIdValues) && correlationIdValues.Count > 0)
        {
            correlationId = correlationIdValues[0];
        }
        else
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Store in HttpContext.Items
        context.Items[CorrelationIdItemKey] = correlationId;

        // Set on response header
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdItemKey = "CorrelationId";

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var correlationId = context.Items.TryGetValue(CorrelationIdItemKey, out var id) ? id?.ToString() : string.Empty;

        int statusCode;
        object errorResponse;

        switch (exception)
        {
            case ArgumentException:
                statusCode = StatusCodes.Status400BadRequest;
                errorResponse = new { error = exception.Message, correlationId };
                break;
            case KeyNotFoundException:
                statusCode = StatusCodes.Status404NotFound;
                errorResponse = new { error = "Resource not found", correlationId };
                break;
            default:
                statusCode = StatusCodes.Status500InternalServerError;
                errorResponse = new { error = "Internal server error", correlationId };
                break;
        }

        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(errorResponse);
        return context.Response.WriteAsync(json);
    }
}

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Call the next middleware
        await _next(context);

        stopwatch.Stop();

        // Add timing header
        context.Response.Headers["Timing"] = $"{stopwatch.ElapsedMilliseconds}ms";

        // Log to ILogger (as requested, though header is primary for testing)
        _logger.LogInformation("Request processed in {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
    }
}