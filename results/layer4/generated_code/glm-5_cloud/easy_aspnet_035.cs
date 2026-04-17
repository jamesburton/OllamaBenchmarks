using System.ComponentModel.DataAnnotations;

public class TenantOptions
{
    [Required]
    public string TenantId { get; set; }

    public string BaseUrl { get; set; } = "https://api.example.com";

    public int TimeoutSeconds { get; set; } = 30;
}

public static class TenantHelper
{
    public static string GetApiUrl(TenantOptions options, string path)
    {
        return $"{options.BaseUrl}/tenants/{options.TenantId}/{path.TrimStart('/')}";
    }
}