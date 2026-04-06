using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

public class ContactModel
{
    [Required] public string Name { get; set; } = "";
    [EmailAddress] public string Email { get; set, set; } = "";
    [Range(1, 120)] public int Age { get; set; }
}

public class ContactFormBase : ComponentBase
{
    public ContactModel Model { get; set; } = new();
    public EditContext EditCtx { get; set; } = null!;
    public bool IsSubmitted { get; set; }

    protected override void OnInitialized()
    {
        EditCtx = new EditContext(Model);
    }

    public void HandleValidSubmit()
    {
        IsSubmitted = true;
    }
}