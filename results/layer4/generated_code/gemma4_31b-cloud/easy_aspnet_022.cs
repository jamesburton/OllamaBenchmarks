using System.ComponentModel.DataAnnotations;

public class JwtOptions
{
    [Required]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 86400)]
    public int ExpirySeconds { get; set; } = 3600;
}