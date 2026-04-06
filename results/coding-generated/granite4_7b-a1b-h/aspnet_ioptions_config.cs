using System.Threading.Tasks;
using System;
using System.Net.Mail;
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
    private readonly IOptions<SmtpOptions> _options;
    public string? LastSentTo { get; set; }

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public SmtpOptions Options => _options.Value;

    public async Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        // Simulate sending email logic here
    }
}