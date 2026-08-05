using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

public class SmtpOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string? Username { get; set; }
}