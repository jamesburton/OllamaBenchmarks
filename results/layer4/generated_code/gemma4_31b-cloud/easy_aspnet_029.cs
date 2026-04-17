using System;
using System.Linq;

public class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    public string[] AllowedMethods { get; set; } = new[] { "GET", "POST" };
    public bool AllowCredentials { get; set; } = false;
}

public static class CorsHelper
{
    public static bool IsOriginAllowed(CorsOptions options, string origin)
    {
        if (options.AllowedOrigins == null || options.AllowedOrigins.Length == 0)
        {
            return true;
        }

        return options.AllowedOrigins.Any(o => string.Equals(o, origin, StringComparison.OrdinalIgnoreCase));
    }
}