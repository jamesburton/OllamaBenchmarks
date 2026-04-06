using Microsoft.Extensions.Options;
using System.Threading.Tasks;

public class SmtpOptions {
    public string Host { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
}

public interface IEmailSender {
    Task SendAsync(string to, string subject, string body);
}

public class SmtpEmailSender : IEmailSender {
    private readonly IOptions<SmtpOptions> _options;

    public string? LastSentTo { get; set; }

    public SmtpOptions Options => _options.Value;

    public SmtpEmailSender(IOptions<SmtpOptions> options) {
        _options = options;
    }

    public Task SendAsync(string to, string subject, string body) {
        LastSentTo = to;
        return Task.CompletedTask;
    }
}

using Microsoft.Extensions.Options;
using System.Threading.Tasks;

public class SmtpOptions {
    public string Host { get; set; } = "";
    public int Port { get; set; } = 0;
    public string? Username { get; set; }
}

public interface IEmailSender {
    Task SendAsync(string to, string subject, string body);
}

public class SmtpEmailSender : IEmailSender {
    private readonly IOptions<SmtpOptions> _options;

    public string? LastSentTo { get; set; }

    public SmtpOptions Options => _options.Value;

    public SmtpEmailSender(IOptions<SmtpOptions> options) {
        _options = options;
    }

    public Task SendAsync(string to, string subject, string body) {
        LastSentTo = to;
        return Task.CompletedTask;
    }
}