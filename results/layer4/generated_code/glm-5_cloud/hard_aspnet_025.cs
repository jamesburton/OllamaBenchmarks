using Microsoft.Extensions.Options;
using System;

namespace AppSettings.VerticalSlice
{
    // Options classes
    public class CachingOptions
    {
        public int DefaultTtlSeconds { get; set; } = 300;
        public int MaxEntries { get; set; } = 1000;
        public bool SlidingExpiration { get; set; } = true;
    }

    public class LoggingOptions
    {
        public string LogLevel { get; set; } = "Information";
        public bool StructuredLogging { get; set; } = true;
        public bool IncludeScopes { get; set; } = false;
    }

    // Composition class (not IOptions)
    public class AppConfig
    {
        public CachingOptions Caching { get; set; } = new CachingOptions();
        public LoggingOptions Logging { get; set; } = new LoggingOptions();
    }

    // Post-configuration to ensure valid values
    public class AppOptionsPostConfigure : IPostConfigureOptions<CachingOptions>
    {
        public void PostConfigure(string name, CachingOptions options)
        {
            if (options.DefaultTtlSeconds < 1)
            {
                options.DefaultTtlSeconds = 1;
            }

            if (options.MaxEntries < 1)
            {
                options.MaxEntries = 1;
            }
        }
    }

    // Service implementation
    public class AppSettingsService
    {
        private readonly IOptions<CachingOptions> _cacheOptions;
        private readonly IOptions<LoggingOptions> _logOptions;

        public AppSettingsService(IOptions<CachingOptions> cacheOptions, IOptions<LoggingOptions> logOptions)
        {
            _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
            _logOptions = logOptions ?? throw new ArgumentNullException(nameof(logOptions));
        }

        public AppConfig GetConfig()
        {
            return new AppConfig
            {
                Caching = _cacheOptions.Value,
                Logging = _logOptions.Value
            };
        }

        public bool IsCachingEnabled()
        {
            var cache = _cacheOptions.Value;
            return cache.DefaultTtlSeconds > 0 && cache.MaxEntries > 0;
        }

        public string GetLogLevel()
        {
            return _logOptions.Value.LogLevel;
        }
    }

    // Extension method for DI registration
    public static class AppSettingsServiceCollectionExtensions
    {
        public static IServiceCollection AddAppSettings(
            this IServiceCollection services,
            Action<CachingOptions>? configureCaching = null,
            Action<LoggingOptions>? configureLogging = null)
        {
            // Register CachingOptions
            services.AddOptions<CachingOptions>()
                .Configure(options =>
                {
                    configureCaching?.Invoke(options);
                })
                .PostConfigure<AppOptionsPostConfigure>();

            // Register LoggingOptions
            services.AddOptions<LoggingOptions>()
                .Configure(options =>
                {
                    configureLogging?.Invoke(options);
                });

            // Register the PostConfigure logic
            services.AddSingleton<IPostConfigureOptions<CachingOptions>, AppOptionsPostConfigure>();

            // Register the service
            services.AddScoped<AppSettingsService>();

            return services;
        }
    }
}