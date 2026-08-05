using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;

namespace BlazorApp
{
    public class ContactModel
    {
        [Required]
        public string Name { get; set; } = "";

        [EmailAddress]
        public string Email { get; set; } = "";

        [Range(1, 120)]
        public int Age { get; set; }
    }

    public partial class ContactFormBase : Microsoft.AspNetCore.Components.ComponentBase
    {
        private ContactModel Model;
        private EditContext EditCtx;

        protected override void OnInitialized()
        {
            EditCtx = new EditContext(Model);
        }

        [Command]
        public void HandleValidSubmit()
        {
            IsSubmitted = true;
        }
    }
}