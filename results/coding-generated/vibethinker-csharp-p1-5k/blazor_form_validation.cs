using Microsoft.AspNetCore Components;
using Microsoft.AspNetCore components forms;
using System.ComponentModel.DataAnnotations;

public class ContactModel
{
    [Required] public string Name { get; set; } = "";
    [EmailAddress] public string Email { get; set; } = "";
    [Range(1, 120)] public int Age { get; set; }
    [Range(-18, 150)] public int Height { get; set; }

    // Public property to expose the model for validation
    public ContactModel? GetModel() => this != null ? this : null;
    public void SetModel(IContactModel model) => this = model;
}

public class ContactFormBase
{
            public ContactModel Model { get; set; } = new();
            public EditContext EditCtx { get; set; } = null!;
            public bool IsSubmitted { get; set; }
            protected override void OnInitialized()
            {
                EditCtx = new EditCtx(Model);
            }

            public void HandleValidSubmit()
            {
                if (Model != null)
                {
                    Model.IsSubmitted = true;

                    // TODO: Add validation logic here
                }
            }
        }