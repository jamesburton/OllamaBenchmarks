using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

// 1. ContactModel class
public class ContactModel
{
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = "";

    [Range(1, 120, ErrorMessage = "Age must be between 1 and 120.")]
    public int Age { get; set; }
}

// 2. ContactFormBase class
public class ContactFormBase : ComponentBase
{
    public ContactModel Model { get; set; } = new();
    public EditContext EditCtx { get; set; } = null!;
    public bool IsSubmitted { get; set; }

    protected override void OnInitialized()
    {
        // Initialize EditContext with the model
        EditCtx = new EditContext(Model);
    }

    public void HandleValidSubmit()
    {
        // Logic executed upon successful form submission
        IsSubmitted = true;
        // In a real application, you would typically call StateHasChanged() here
    }
}