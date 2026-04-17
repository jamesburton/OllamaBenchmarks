using System;

public class DatabaseOptions
{
    public string ConnectionString { get; set; }
    public int CommandTimeout { get; set; } = 30;
    public int MaxPoolSize { get; set; } = 10;
}

public static class DatabaseOptionsValidator
{
    public static bool IsValid(DatabaseOptions options)
    {
        if (options == null) return false;

        return !string.IsNullOrWhiteSpace(options.ConnectionString) 
               && options.CommandTimeout > 0 
               && options.MaxPoolSize > 0;
    }
}