using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EmailServiceSlice
{
    // 1. Typed Configuration Class
    public class EmailServiceOptions
    {
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public bool UseTls { get; set; } = true;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    // 2. Options Validator
    public class EmailServiceOptionsValidator : IValidateOptions<EmailServiceOptions>
    {
        public ValidateOptionsResult Validate(string? name, EmailServiceOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.SmtpHost))
            {
                return ValidateOptionsResult.Fail("SmtpHost must be provided.");
            }

            if (options.SmtpPort < 1 || options.SmtpPort > 65535)
            {
                return ValidateOptionsResult.Fail("SmtpPort must be between 1 and 65535.");
            }

            if (string.IsNullOrWhiteSpace(options.FromAddress) || !options.FromAddress.Contains("@"))
            {
                return ValidateOptionsResult.Fail("FromAddress must contain '@'.");
            }

            return ValidateOptionsResult.Success;
        }
    }

    // 3. Service Implementation
    public class EmailService
    {
        private readonly IOptionsMonitor<EmailServiceOptions> _optionsMonitor;

        public EmailServiceOptions CurrentOptions => _optionsMonitor.CurrentValue;
        public event EventHandler<EmailServiceOptions>? OptionsChanged;

        public EmailService(IOptionsMonitor<EmailServiceOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;

            // Subscribe to changes
            _optionsMonitor.OnChange(newOptions => 
            {
                OptionsChanged?.Invoke(this, newOptions);
            });
        }

        public Task<bool> SendAsync(string to, string subject, string body, CancellationToken ct)
        {
            // Validate recipient email format
            if (string.IsNullOrWhiteSpace(to) || !to.Contains("@"))
            {
                return Task.FromResult(false);
            }

            // Get current options snapshot
            var options = _optionsMonitor.CurrentValue;

            // Validate options at send time
            var validator = new EmailServiceOptionsValidator();
            var validationResult = validator.Validate(name: null, options);

            if (validationResult.Failed)
            {
                // In a real scenario, you might log validationResult.FailureMessage
                return Task.FromResult(false);
            }

            // Construct email metadata (simulated)
            // In a real implementation, this would connect to the SMTP server using the options.
            // var emailMetadata = new 
            // {
            //     Host = options.SmtpHost,
            //     Port = options.SmtpPort,
            //     UseTls = options.UseTls,
            //     From = $"{options.FromName} <{options.FromAddress}>",
            //     To = to,
            //     Subject = subject,
            //     Body = body
            // };

            // Simulate successful send
            return Task.FromResult(true);
        }
    }

    // 4. Extension Method for DI Registration
    public static class EmailServiceExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services, Action<EmailServiceOptions> configure)
        {
            // Configure options
            services.Configure(configure);

            // Register validator
            services.AddSingleton<IValidateOptions<EmailServiceOptions>, EmailServiceOptionsValidator>();

            // Register service
            services.AddSingleton<EmailService>();

            return services;
        }
    }
}