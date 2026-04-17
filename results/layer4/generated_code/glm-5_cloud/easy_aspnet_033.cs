using System;

public class LoggingOptions
{
    public string MinimumLevel { get; set; } = "Information";
    public bool EnableConsole { get; set; } = true;
    public bool EnableFile { get; set; } = false;
    public string? FilePath { get; set; }
}

public static class LoggingOptionsHelper
{
    public static bool RequiresFilePath(LoggingOptions options)
    {
        if (options == null)
        {
            return false;
        }

        return options.EnableFile && string.IsNullOrEmpty(options.FilePath);
    }
}