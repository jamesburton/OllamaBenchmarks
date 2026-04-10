using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

// 1. SmtpOptions class
public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? Username { get; set; }
}

// 2. IEmailSender interface
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

// 3. SmtpEmailSender class
public class SmtpEmailSender : IEmailSender
{
    private readonly IOptions<SmtpOptions> _options;

    // Public property to expose configuration for testing
    public SmtpOptions Options => _options.Value;

    // Public property to track the last recipient
    public string? LastSentTo { get; set; }

    // Constructor taking IOptions<SmtpOptions>
    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options;
    }

    public Task SendAsync(string to, string subject, string body)
    {
        // Implementation detail: store 'to' in LastSentTo
        LastSentTo = to;

        // Simulate sending logic
        Console.WriteLine($"Sending email to {to} with subject: {subject}");

        return Task.CompletedTask;
    }
}