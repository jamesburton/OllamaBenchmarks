using Microsoft.Extensions.Options;

namespace EmailService;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

public class SmtpOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
}

public class SmtpEmailSender : IEmailSender
{
    public SmtpOptions Options { get; }
    public string? LastSentTo { get; set; }

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        Options = options.Value;
        LastSentTo = null;
    }

    public async Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        await Task.CompletedTask;
    }
}