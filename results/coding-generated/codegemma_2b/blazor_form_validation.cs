public class ContactFormBase : ComponentBase
{
    public ContactModel Model { get; set; } = new();
    public EditContext EditCtx { get; set; } = null!