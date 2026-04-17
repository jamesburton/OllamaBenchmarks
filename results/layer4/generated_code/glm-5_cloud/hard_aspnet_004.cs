using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DatabaseServices
{
    // Configuration options class with DataAnnotations for declarative validation rules
    public class DatabaseOptions
    {
        private const int DefaultMaxPoolSize = 100;
        private const int DefaultRetryCount = 3;
        private static readonly TimeSpan DefaultCommandTimeout = TimeSpan.FromSeconds(30);

        [Required]
        public string ConnectionString { get; set; } = string.Empty;

        [Range(1, 500)]
        public int MaxPoolSize { get; set; } = DefaultMaxPoolSize;

        [Range(typeof(TimeSpan), "00:00:01", "00:05:00")]
        public TimeSpan CommandTimeout { get; set; } = DefaultCommandTimeout;

        public bool EnableRetry { get; set; } = true;

        [Range(1, 10)]
        public int RetryCount { get; set; } = DefaultRetryCount;
    }

    // Custom validator to handle complex logic (conditional validation) and aggregate error messages
    public class DatabaseOptionsValidator : IValidateOptions<DatabaseOptions>
    {
        public ValidateOptionsResult Validate(string? name, DatabaseOptions options)
        {
            var failures = new List<string>();

            // Validate ConnectionString (Required, not empty)
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                failures.Add("ConnectionString is required and cannot be empty.");
            }

            // Validate MaxPoolSize (1-500)
            if (options.MaxPoolSize < 1 || options.MaxPoolSize > 500)
            {
                failures.Add($"MaxPoolSize must be between 1 and 500. Provided: {options.MaxPoolSize}.");
            }

            // Validate CommandTimeout (1s-300s)
            var minTimeout = TimeSpan.FromSeconds(1);
            var maxTimeout = TimeSpan.FromSeconds(300);

            if (options.CommandTimeout < minTimeout || options.CommandTimeout > maxTimeout)
            {
                failures.Add($"CommandTimeout must be between 1s and 300s. Provided: {options.CommandTimeout}.");
            }

            // Validate RetryCount (1-10) ONLY if EnableRetry is true
            if (options.EnableRetry)
            {
                if (options.RetryCount < 1 || options.RetryCount > 10)
                {
                    failures.Add($"RetryCount must be between 1 and 10 when EnableRetry is true. Provided: {options.RetryCount}.");
                }
            }

            if (failures.Count > 0)
            {
                return ValidateOptionsResult.Fail(string.Join(" ", failures));
            }

            return ValidateOptionsResult.Success;
        }
    }

    // Service consuming the validated options
    public class DatabaseService
    {
        private readonly DatabaseOptions _options;

        public DatabaseService(IOptions<DatabaseOptions> options)
        {
            // Options are guaranteed to be valid here if validation runs during configuration
            _options = options.Value;
        }

        public string GetConnectionString() => _options.ConnectionString;

        public int GetEffectivePoolSize() => Math.Min(_options.MaxPoolSize, 200);

        public bool IsRetryEnabled() => _options.EnableRetry;
    }

    // Static extension method for DI registration
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDatabaseService(this IServiceCollection services, Action<DatabaseOptions> configure)
        {
            // 1. Register the options configuration
            services.Configure<DatabaseOptions>(configure);

            // 2. Register the validator
            services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

            // 3. Register the service
            services.AddTransient<DatabaseService>();

            // Optional: Enforce validation at startup (requires calling ValidateOnStart)
            // services.Options<DatabaseOptions>().ValidateOnStart(); 
            // Note: ValidateOnStart requires Microsoft.Extensions.Options.DataAnnotations or manual validation triggering in .NET 6/7+ generic host.
            // For a raw ServiceCollection, validation typically runs when IOptions<T> is accessed.

            return services;
        }
    }

    // Dummy attribute classes to ensure the code compiles without referencing System.ComponentModel.DataAnnotations
    // (In a real project, these would come from the System.ComponentModel.Annotations NuGet package)
    internal sealed class RequiredAttribute : Attribute { }
    internal sealed class RangeAttribute : Attribute
    {
        public RangeAttribute(int min, int max) { }
        public RangeAttribute(Type type, string min, string max) { }
    }
}