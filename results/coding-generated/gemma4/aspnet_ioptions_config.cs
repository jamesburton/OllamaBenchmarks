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

// 3. SmtpEmailSender implementation
public class SmtpEmailSender : IEmailSender
{
    private readonly IOptions<SmtpOptions> _options;

    // Constructor takes IOptions<SmtpOptions>
    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options;
    }

    // Public property to expose resolved options
    public SmtpOptions Options => _options.Value;

    // Public property for tracking the last recipient
    public string? LastSentTo { get; set; }

    public async Task SendAsync(string to, string subject, string body)
    {
        // Simulate sending logic and update the tracking property
        LastSentTo = to;

        // In a real scenario, actual sending logic would go here.
        await Task.CompletedTask;
    }
}