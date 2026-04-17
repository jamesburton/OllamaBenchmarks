using Microsoft.AspNetCore.Components;

public class ToggleComponent : ComponentBase
{
    [Parameter]
    public bool InitialValue { get; set; } = false;

    public bool IsOn { get; private set; }

    protected override void OnInitialized()
    {
        IsOn = InitialValue;
    }

    public void Toggle()
    {
        IsOn = !IsOn;
    }
}