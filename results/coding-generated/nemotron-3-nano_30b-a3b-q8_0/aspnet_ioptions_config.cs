using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

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
    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        Options = options.Value;
    }

    public string? LastSentTo { get; set; }

    public SmtpOptions Options { get; }

    public async Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        return await Task.CompletedTask;
    }
}