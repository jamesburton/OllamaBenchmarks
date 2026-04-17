using Microsoft.AspNetCore.Components;

public class TimerDisplayComponent : ComponentBase
{
    [Parameter]
    public int TotalSeconds { get; set; }

    protected string Display => $"{TotalSeconds / 60:D2}:{TotalSeconds % 60:D2}";

    protected bool IsExpired => TotalSeconds <= 0;
}