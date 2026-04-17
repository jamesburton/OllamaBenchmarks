public class AppOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetries { get; set; }
    public bool EnableCaching { get; set; }
}

public static class OptionsValidator
{
    public static (bool IsValid, string? Error) Validate(AppOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return (false, "ConnectionString is required");
        }

        if (options.MaxRetries <= 0)
        {
            return (false, "MaxRetries must be positive");
        }

        return (true, null);
    }
}