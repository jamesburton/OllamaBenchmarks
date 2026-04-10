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
    public SmtpOptions Options { get; }
    public string? LastSentTo { get; set; }

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        Options = options.Value;
    }

    public Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        return Task.CompletedTask;
    }
}