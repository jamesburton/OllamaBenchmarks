using System.ComponentModel.DataAnnotations;

public class CreateUserRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Range(1, 120)]
    public int Age { get; set; }
}