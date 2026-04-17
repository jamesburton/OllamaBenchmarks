using System.ComponentModel.DataAnnotations;

public class PasswordRequest
{
    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; }
}