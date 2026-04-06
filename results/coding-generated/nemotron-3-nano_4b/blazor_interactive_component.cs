using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    [Parameter] public int InitialCount { get; set; } = 0;
    [Parameter] public EventCallback<int> OnCountChanged { get; set; } = default!;

    private int currentCount;

    protected override void OnInitialized()
    {
        currentCount = InitialCount ?? 0;
    }

    public async Task IncrementCount()
    {
        await OnCountChanged.InvokeAsync(currentCount++);
    }

    public int CurrentCount => currentCount;
}