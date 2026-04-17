using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SecurityHeaders.Extensions
{
    /// <summary>
    /// Options for security headers configuration.
    /// </summary>
    public class SecurityHeadersOptions
    {
        public bool EnableHsts { get; set; } = true;
        public bool EnableXFrameOptions { get; set; } = true;
        public string XContentTypeOptions { get; set; } = "nosniff";
        public bool EnableCsp { get; set; } = false;
        public string CspPolicy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Startup filter to register the SecurityHeadersMiddleware.
    /// </summary>
    public class SecurityHeadersStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                // Add the middleware early in the pipeline
                builder.UseMiddleware<SecurityHeadersMiddleware>();
                next(builder);
            };
        }
    }

    /// <summary>
    /// Middleware that adds security headers to the response.
    /// </summary>
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IOptions<SecurityHeadersOptions> optionsAccessor)
        {
            var options = optionsAccessor.Value;

            if (options.EnableHsts)
            {
                context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
            }

            if (options.EnableXFrameOptions)
            {
                context.Response.Headers["X-Frame-Options"] = "DENY";
            }

            if (!string.IsNullOrEmpty(options.XContentTypeOptions))
            {
                context.Response.Headers["X-Content-Type-Options"] = options.XContentTypeOptions;
            }

            if (options.EnableCsp && !string.IsNullOrEmpty(options.CspPolicy))
            {
                context.Response.Headers["Content-Security-Policy"] = options.CspPolicy;
            }

            await _next(context);
        }
    }

    /// <summary>
    /// Post-configure options to ensure defaults and validation.
    /// </summary>
    public class SecurityHeadersPostConfigureOptions : IPostConfigureOptions<SecurityHeadersOptions>
    {
        public void PostConfigure(string name, SecurityHeadersOptions options)
        {
            // Ensure CspPolicy is not null
            options.CspPolicy ??= string.Empty;

            // Ensure XContentTypeOptions is trimmed
            if (options.XContentTypeOptions != null)
            {
                options.XContentTypeOptions = options.XContentTypeOptions.Trim();
            }
        }
    }

    /// <summary>
    /// Extension methods for service collection.
    /// </summary>
    public static class SecurityHeadersServiceCollectionExtensions
    {
        public static IServiceCollection AddSecurityHeaders(this IServiceCollection services, Action<SecurityHeadersOptions>? configure = null)
        {
            // Register options
            services.Configure<SecurityHeadersOptions>(options =>
            {
                configure?.Invoke(options);
            });

            // Register post-configure
            services.AddSingleton<IPostConfigureOptions<SecurityHeadersOptions>, SecurityHeadersPostConfigureOptions>();

            // Register startup filter
            services.AddSingleton<IStartupFilter, SecurityHeadersStartupFilter>();

            return services;
        }
    }
}