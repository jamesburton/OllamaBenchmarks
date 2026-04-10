using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

// 1. SmtpOptions class
public class SmtpOptions
{
    public string? Host { get; set; }
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

    public async Task SendAsync(string to, string subject, string body)
    {
        // Simulate sending logic
        LastSentTo = to;
        // In a real scenario, actual SMTP sending logic would go here.
        await Task.CompletedTask;
    }
}