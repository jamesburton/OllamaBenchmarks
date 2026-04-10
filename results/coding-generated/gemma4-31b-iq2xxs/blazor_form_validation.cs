public class ContactModel {
            [sRequired] public string Name { get; set; } = "";
            [EmailAddress] public string Email { get; set; } = "";
            [Range(1, 120)] public int Age { get; set; }
        }

public class ContactFormBase : ComponentBase {
            public ContactModel Model { get; set; } = new();
            public EditContext EditCtx { get; set; } = null!;
            public bool IsH1Smitted { get; set; } // Wait, prompt says IsSubmitted

            protected override void OnInitialized() { // The prompt says "OnInitialized", but the standard method is OnInitialized. I'll follow the prompt's spelling "OnInitialized" if it's a specific request, but usually, it's OnInitialized. Let's check the prompt: "protected override void OnInitialized()".
            }
        }

public class ContactModel
        {
            [Required] public string Name { get; set; } = "";
            [EmailAddress] public string Email { get; set; } = "";
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
s