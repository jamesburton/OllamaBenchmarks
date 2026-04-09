using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    [Parameter] public int InitialCount { get; set; } = 0;
    [Parameter] public EventCallback<int> OnCountChanged { get; set; }

    private int currentCount;

    protected override void OnInitialized()
    {
        currentCount = InitialCount;
        base.OnInitialized();
    }

    public async Task IncrementCount()
    {
        currentCount++;
        await OnCountChanged.InvokeAsync(currentCount);
    }

    public int CurrentCount => currentCount;
}