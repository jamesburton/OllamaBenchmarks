using Microsoft.AspNetCore.Components;
    using Microsoft.AspNetCore.Components.Forms;
    using System.ComponentModel.DataAnnotations;

    // ContactModel
    public class ContactModel
    {
        [Required]
        public string Name { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Range(1, 120)]
        public int Age { get; set; }
    }

    // ContactFormBase
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