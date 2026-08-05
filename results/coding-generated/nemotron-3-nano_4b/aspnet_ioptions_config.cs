using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? Username { get; set; }
}

interface IEmailSender
{
    Task SendAsync(string to, string subject, string body);
}

class SmtpEmailSender : IEmailSender
{
    public string? LastSentTo { get; set; }
    public SmtpOptions Options { get; }

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

class Program {}

services.AddHttpClient<IGitHubClient, GitHubClient>(c => c.BaseAddress = new Uri("https://api.github.com/"));

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

var builder = builder;