public class SmtpOptions
{
    public string Host { get; set; }
    public int Port { get; set; } = 587;
    public string Username { get; set; }
    public string Password { get; set; }
    public bool UseSsl { get; set; } = true;
}

public static class SmtpOptionsHelper
{
    public static string GetConnectionInfo(SmtpOptions options)
    {
        return $"{options.Host}:{options.Port} (ssl={options.UseSsl})";
    }
}