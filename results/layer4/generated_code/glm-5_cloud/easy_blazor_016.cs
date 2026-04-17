using Microsoft.AspNetCore.Components;

public class AccordionComponent : ComponentBase
{
    [Parameter]
    public string Title { get; set; } = "";

    protected bool IsExpanded { get; set; } = false;

    protected void Toggle()
    {
        IsExpanded = !IsExpanded;
    }
}