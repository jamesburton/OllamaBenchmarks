using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FeatureManagement
{
    // 1. Options Class
    public class FeatureFlagOptions
    {
        public Dictionary<string, bool> Flags { get; set; } = new Dictionary<string, bool>();
    }

    // 2. Interface
    public interface IFeatureManager
    {
        bool IsEnabled(string featureName);
        void Override(string featureName, bool enabled);
        IReadOnlyDictionary<string, bool> GetAllFlags();
    }

    // 3. Implementation
    public class FeatureManager : IFeatureManager
    {
        private readonly IOptionsMonitor<FeatureFlagOptions> _optionsMonitor;
        private readonly Dictionary<string, bool> _runtimeOverrides = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public FeatureManager(IOptionsMonitor<FeatureFlagOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }

        public bool IsEnabled(string featureName)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                return false;
            }

            // Runtime overrides take precedence
            if (_runtimeOverrides.TryGetValue(featureName, out bool runtimeValue))
            {
                return runtimeValue;
            }

            // Fallback to configuration
            var options = _optionsMonitor.CurrentValue;
            if (options?.Flags != null && options.Flags.TryGetValue(featureName, out bool configValue))
            {
                return configValue;
            }

            // Default to false if not found
            return false;
        }

        public void Override(string featureName, bool enabled)
        {
            if (!string.IsNullOrWhiteSpace(featureName))
            {
                _runtimeOverrides[featureName] = enabled;
            }
        }

        public IReadOnlyDictionary<string, bool> GetAllFlags()
        {
            // Merge configuration flags with runtime overrides
            var allFlags = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

            var options = _optionsMonitor.CurrentValue;
            if (options?.Flags != null)
            {
                foreach (var flag in options.Flags)
                {
                    allFlags[flag.Key] = flag.Value;
                }
            }

            // Apply overrides
            foreach (var flag in _runtimeOverrides)
            {
                allFlags[flag.Key] = flag.Value;
            }

            return allFlags;
        }
    }

    // 4. Attribute
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FeatureRequiredAttribute : Attribute
    {
        public string FeatureName { get; }

        public FeatureRequiredAttribute(string featureName)
        {
            if (string.IsNullOrWhiteSpace(featureName))
            {
                throw new ArgumentNullException(nameof(featureName));
            }
            FeatureName = featureName;
        }
    }

    // 5. Endpoint Filter
    public class FeatureRequiredFilter : IEndpointFilter
    {
        private readonly string _featureName;
        private readonly IFeatureManager _featureManager;

        public FeatureRequiredFilter(string featureName, IFeatureManager featureManager)
        {
            _featureName = featureName;
            _featureManager = featureManager;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            if (!_featureManager.IsEnabled(_featureName))
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                return Results.Problem("Feature disabled", statusCode: 503);
            }

            return await next(context);
        }
    }

    // 6. Service Collection Extensions
    public static class FeatureManagementServiceExtensions
    {
        public static IServiceCollection AddFeatureManagement(this IServiceCollection services, Action<FeatureFlagOptions>? configure = null)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }
            else
            {
                // Ensure options are registered even if no configuration action is provided
                services.AddOptions<FeatureFlagOptions>();
            }

            services.AddSingleton<IFeatureManager, FeatureManager>();

            return services;
        }
    }

    // 7. Route Handler Builder Extension
    public static class FeatureManagementEndpointExtensions
    {
        public static RouteHandlerBuilder RequireFeature(this RouteHandlerBuilder builder, string featureName)
        {
            builder.AddEndpointFilter(async (context, next) =>
            {
                var featureManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureManager>();
                var filter = new FeatureRequiredFilter(featureName, featureManager);
                return await filter.InvokeAsync(context, next);
            });

            return builder;
        }
    }
}