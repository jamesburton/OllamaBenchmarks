using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

public class SmtpOptions
{
    public string Host { get; set; } = null!;
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
    public string? LastSentTo { get; private set; }

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public SmtpOptions Options => _options;

    public Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        return Task.CompletedTask;
    }
}