using System.ComponentModel.DataAnnotations;

public class JwtOptions
{
    [Required]
    public string Secret { get; set; }

    [Required]
    public string Issuer { get; set; }

    [Required]
    public string Audience { get; set; }

    [Range(1, 86400)]
    public int ExpirySeconds { get; set; } = 3600;
}