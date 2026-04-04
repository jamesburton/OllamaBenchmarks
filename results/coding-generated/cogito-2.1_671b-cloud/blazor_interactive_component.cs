using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    private int currentCount;

    [Parameter]
    public int InitialCount { get; set; }

    [Parameter]
    public EventCallback<int> OnCountChanged { get; set; }

    public int CurrentCount => currentCount;

    protected override void OnInitialized()
    {
        currentCount = InitialCount;
    }

    public async Task IncrementCount()
    {
        currentCount++;
        await OnCountChanged.InvokeAsync(currentCount);
    }
}