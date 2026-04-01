using Microsoft.AspNetCore.Components;

public class CounterBase : Microsoft.AspNetCore.Components.ComponentBase
{
    [Parameter] public int InitialCount { get; set; } = 0;
    [Parameter] public EventCallback<int> OnCountChanged { get; set; } = default!;

    private int currentCount;
    public int CurrentCount => currentCount;

    public async Task IncrementCount()
    {
        currentCount++;
        await OnCountChanged.InvokeAsync(currentCount);
        await Task.CompletedTask;
    }

    protected override void OnInitialized()
    {
        currentCount = InitialCount;
    }
}