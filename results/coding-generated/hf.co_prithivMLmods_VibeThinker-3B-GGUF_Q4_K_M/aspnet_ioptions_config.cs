using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions Options;
using Microsoft.Extensions Configuration;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

using Microsoft.Extensions options;
using Microsoft.Extensions configuration;

namespace ...? No namespace. So we cannot have namespace statements. Just top-level types.

class SmtpOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
}

interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

class SmtpEmailSender
{
    private readonly IOptions<SmtpOptions> Options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        Options = options;
    }

    public string? LastSentTo { get; set; }

    public SmtpOptions Options => Options.Value;

    public async Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        return Task.CompletedTask;
    }
}

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

class SmtpOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
}

interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

class SmtpEmailSender
{
    private readonly IOptions<SmtpOptions> Options;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
    {
        Options = options;
    }

    public string? LastSentTo { get; set; }

    public SmtpOptions Options => Options.Value;

    public async Task SendAsync(string to, string subject, string body)
    {
        LastSentTo = to;
        return Task.CompletedTask;
    }
}