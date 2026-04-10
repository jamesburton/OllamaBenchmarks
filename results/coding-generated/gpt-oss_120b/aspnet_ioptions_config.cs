using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

public class SmtpOptions
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string? Username { get; set; }
}

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly IOptions<SmtpOptions> _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public string? LastSentTo { get; set; }

    public SmtpOptions Options => _options.Value;

    public Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        // In a real implementation, SMTP logic would go here.
        return Task.CompletedTask;
    }
}