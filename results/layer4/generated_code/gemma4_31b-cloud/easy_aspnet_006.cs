using System.ComponentModel.DataAnnotations;

public class EmailRequest
{
    [Required]
    [EmailAddress]
    public string To { get; set; }

    [Required]
    [MinLength(1)]
    public string Subject { get; set; }

    [Required]
    public string Body { get; set; }
}