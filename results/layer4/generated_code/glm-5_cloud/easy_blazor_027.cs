using Microsoft.AspNetCore.Components;

public class BadgeComponent : ComponentBase
{
    [Parameter]
    public int Count { get; set; }

    [Parameter]
    public int MaxDisplay { get; set; } = 99;

    protected string DisplayText => Count > MaxDisplay ? $"{MaxDisplay}+" : Count.ToString();

    protected bool HasItems => Count > 0;
}