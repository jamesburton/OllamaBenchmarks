using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations;

[RenderMode(InteractionServer)]
public class ContactFormBase : ComponentBase
{
    public ContactModel Model { get; set; } = new();
    public EditContext EditCtx { get; set; } = null!;
    public bool IsSubmitted { get; set; }

    protected override void OnInitialized()
    {
        EditCtx = new EditContext(Model);
        EditCtx.EnableValidationMessagesAsHtml();
    }

    public void HandleValidSubmit()
    {
        IsSubmitted = true;
    }

    public override void OnAfterRender(string? firstRender)
    {
        if (!IsSubmitted && EditCtx.IsValid)
        {
            HandleValidSubmit();
        }
    }
}

internal class ContactModel
{
    [Required] public string Name { get; set; } = "";
    [EmailAddress] public string Email { get; set; } = "";
    [Range(1, 120)] public int Age { get; set; }
}