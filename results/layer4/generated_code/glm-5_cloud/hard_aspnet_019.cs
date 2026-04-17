using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace VerticalSliceDemo
{
    // 1. Interface Definition
    public interface IMessageSender
    {
        Task SendAsync(string to, string subject, string body, CancellationToken ct);
    }

    // 2. Implementations
    public class SmtpMessageSender : IMessageSender
    {
        public List<string> SentMessages { get; } = new List<string>();

        public Task SendAsync(string to, string subject, string body, CancellationToken ct)
        {
            SentMessages.Add($"[SMTP] To: {to}, Subject: {subject}");
            return Task.CompletedTask;
        }
    }

    public class SmsMessageSender : IMessageSender
    {
        public List<string> SentMessages { get; } = new List<string>();

        public Task SendAsync(string to, string subject, string body, CancellationToken ct)
        {
            SentMessages.Add($"[SMS] To: {to}, Subject: {subject}");
            return Task.CompletedTask;
        }
    }

    public class SlackMessageSender : IMessageSender
    {
        public List<string> SentMessages { get; } = new List<string>();

        public Task SendAsync(string to, string subject, string body, CancellationToken ct)
        {
            SentMessages.Add($"[SLACK] To: {to}, Subject: {subject}");
            return Task.CompletedTask;
        }
    }

    // 3. Consumer Service
    public class NotificationService
    {
        private readonly IEnumerable<IMessageSender> _allSenders;
        private readonly IServiceProvider _serviceProvider;

        public NotificationService(IEnumerable<IMessageSender> senders, IServiceProvider serviceProvider)
        {
            _allSenders = senders;
            _serviceProvider = serviceProvider;
        }

        public async Task NotifyAllAsync(string to, string subject, string body, CancellationToken ct)
        {
            foreach (var sender in _allSenders)
            {
                await sender.SendAsync(to, subject, body, ct);
            }
        }

        public async Task NotifyViaAsync(string channel, string to, string subject, string body, CancellationToken ct)
        {
            // Retrieve the specific keyed service
            var sender = _serviceProvider.GetRequiredKeyedService<IMessageSender>(channel);

            if (sender == null)
            {
                throw new ArgumentException($"Unknown channel: {channel}");
            }

            await sender.SendAsync(to, subject, body, ct);
        }
    }

    // 4. DI Registration Extension
    public static class NotificationServiceExtensions
    {
        public static IServiceCollection AddNotificationServices(this IServiceCollection services)
        {
            // Register keyed services
            services.AddKeyedTransient<IMessageSender, SmtpMessageSender>("smtp");
            services.AddKeyedTransient<IMessageSender, SmsMessageSender>("sms");
            services.AddKeyedTransient<IMessageSender, SlackMessageSender>("slack");

            // Register the main service
            services.AddTransient<NotificationService>();

            return services;
        }
    }

    // 5. Demonstration Entry Point
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddNotificationServices();

            var provider = services.BuildServiceProvider();

            // Scenario 1: Resolve service and Notify All
            var notificationService = provider.GetRequiredService<NotificationService>();

            Console.WriteLine("--- Sending to all channels ---");
            await notificationService.NotifyAllAsync("user@example.com", "Welcome", "Hello World", CancellationToken.None);

            // Scenario 2: Notify via specific channel (Keyed Service)
            Console.WriteLine("\n--- Sending via SMS channel ---");
            await notificationService.NotifyViaAsync("sms", "user@example.com", "Alert", "System Alert", CancellationToken.None);

            // Scenario 3: Verify Lifetime and Tracking
            Console.WriteLine("\n--- Verifying Sent Messages ---");

            // We resolve the specific keyed services directly to check their internal lists
            var smtpSender = provider.GetRequiredKeyedService<IMessageSender>("smtp") as SmtpMessageSender;
            var smsSender = provider.GetRequiredKeyedService<IMessageSender>("sms") as SmsMessageSender;
            var slackSender = provider.GetRequiredKeyedService<IMessageSender>("slack") as SlackMessageSender;

            Console.WriteLine($"SMTP Sender tracked: {smtpSender?.SentMessages.Count ?? 0} message(s)");
            Console.WriteLine($"SMS Sender tracked: {smsSender?.SentMessages.Count ?? 0} message(s)");
            Console.WriteLine($"Slack Sender tracked: {slackSender?.SentMessages.Count ?? 0} message(s)");

            // Scenario 4: Unknown channel exception
            Console.WriteLine("\n--- Testing Unknown Channel ---");
            try
            {
                await notificationService.NotifyViaAsync("fax", "user@example.com", "Fail", "Should fail", CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Caught expected exception: {ex.GetType().Name}");
            }
        }
    }
}