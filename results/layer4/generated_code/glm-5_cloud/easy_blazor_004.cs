using Microsoft.AspNetCore.Components;

public class UserCardComponent : ComponentBase
{
    [Parameter]
    public string FirstName { get; set; } = "";

    [Parameter]
    public string LastName { get; set; } = "";

    [Parameter]
    public string Email { get; set; } = "";

    public string FullName => $"{FirstName} {LastName}".Trim();

    public bool HasEmail => !string.IsNullOrEmpty(Email);
}