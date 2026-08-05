using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? Username { get; set; }
}

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    public string? LastSentTo { get; set; }

    public SmtpOptions Options => _options.Value;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public Task SendAsync(string to, string subject, string body)
    {
        // Simulate sending logic
        Console.WriteLine($"Attempting to send email via {_options.Host}:{_options.Port} for user {_options.Username} to {to}");

        // Store the recipient for tracking
        LastSentTo = to;

        // Return completed task as required
        return Task.CompletedTask;
    }
}