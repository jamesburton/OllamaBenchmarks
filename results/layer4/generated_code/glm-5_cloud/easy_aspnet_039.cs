using System.ComponentModel.DataAnnotations;

public class ContactRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [Phone]
    public string PhoneNumber { get; set; }

    [Url]
    public string? Website { get; set; }
}