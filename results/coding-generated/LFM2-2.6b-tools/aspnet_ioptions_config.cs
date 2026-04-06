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
    private readonly SmtpOptions _options;
    public string? LastSentTo { get; set; }
    public SmtpOptions Options => _options;

    public SmtpEmailSender(IOptions<SmtpOptions> options) => _options = options.Value;

    public async Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        await Task.CompletedTask;
    }
}