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
        if (options == null)
        {
            return false;
        }

        // Return true if AllowedOrigins is empty (allow all)
        if (options.AllowedOrigins == null || options.AllowedOrigins.Length == 0)
        {
            return true;
        }

        // Return true if AllowedOrigins contains origin (case-insensitive)
        for (int i = 0; i < options.AllowedOrigins.Length; i++)
        {
            if (string.Equals(options.AllowedOrigins[i], origin, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}