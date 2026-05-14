using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public class SmtpOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
}

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

public class SmtpEmailSender : IEmailSender
{
    public string? LastSentTo { get; set; }
    public SmtpOptions Options => options.Value;

    private readonly IOptions<SmtpOptions> options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        this.options = options;
    }

    public Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        return Task.CompletedTask;
    }
}