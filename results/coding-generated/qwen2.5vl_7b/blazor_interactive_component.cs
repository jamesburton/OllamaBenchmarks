using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    [Parameter] public int InitialCount { get; set; } = 0;

    [Parameter] public EventCallback<int> OnCountChanged { get; set; }

    private int currentCount;

    protected override void OnInitialized()
    {
        currentCount = InitialCount;
    }

    public int CurrentCount => currentCount;

    public void IncrementCount()
    {
        currentCount++;
        OnCountChanged.InvokeAsync(currentCount);
    }
}