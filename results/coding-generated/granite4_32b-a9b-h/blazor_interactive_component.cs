using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    [Parameter]
    public int InitialCount { get; set; }

    [Parameter]
    public EventCallback<int> OnCountChanged { get; set; }

    private int currentCount;
    private async Task IncrementCount()
    {
        currentCount++;
        await OnCountChanged.InvokeAsync(currentCount);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        currentCount = InitialCount;
    }

    public int CurrentCount => currentCount;
}