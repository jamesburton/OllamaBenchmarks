using System;
using System.ComponentModel.DataAnnotations;

public class ApiKeyOptions
{
    [Required]
    public string ApiKey { get; set; } = string.Empty;
    public string[] AllowedIps { get; set; } = Array.Empty<string>();
    public bool RequireHttps { get; set; } = true;
}

public static class ApiKeyHelper
{
    public static bool IsKeyValid(ApiKeyOptions options, string providedKey)
    {
        if (options == null || string.IsNullOrEmpty(options.ApiKey))
        {
            return false;
        }

        return options.ApiKey == providedKey;
    }
}