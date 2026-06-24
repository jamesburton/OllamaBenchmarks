using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

public class SmtpOptions
{
    public string Host { get; set; } = null!;

    [Range(1, 65535)]
    public int Port { get; set; }

    public string? Username { get; set; }
}

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly OptionsMonitor<SmtpOptions> _optionsMonitor;

    public string? LastSentTo { get; set; }

    public SmtpOptions Options => _optionsMonitor.CurrentValue;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        var monitor = new OptionsMonitor<SmtpOptions>(options);
        this._optionsMonitor = monitor;

        // Initialize with current value to ensure property is accessible immediately
        _ = monitor.CurrentValue; 
    }

    public Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        return Task.CompletedTask;
    }
}