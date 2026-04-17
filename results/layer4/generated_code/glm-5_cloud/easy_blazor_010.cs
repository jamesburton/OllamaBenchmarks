using Microsoft.AspNetCore.Components;

public class FormComponent : ComponentBase
{
    protected string Username { get; set; } = "";
    protected string Password { get; set; } = "";

    protected string? ErrorMessage { get; private set; }

    protected bool IsValid => !string.IsNullOrWhiteSpace(Username) && Password.Length >= 8;

    protected void Validate()
    {
        if (IsValid)
        {
            ErrorMessage = null;
        }
        else
        {
            ErrorMessage = "Invalid credentials";
        }
    }
}