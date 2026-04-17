public class Config
{
    public string Host { get; init; }
    public int Port { get; init; }
    public bool UseSsl { get; init; }
}

public static class ConfigHelper
{
    public static string ToConnectionString(Config config)
    {
        return $"{config.Host}:{config.Port}?ssl={config.UseSsl.ToString().ToLower()}";
    }
}