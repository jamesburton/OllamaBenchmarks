using Microsoft.AspNetCoreComponents;

public class CounterBase : ComponentBase
{
    [Parameter]
    public int InitialCount { get; set; }

    [Parameter]
    public EventCallback<int> OnCountChanged { get; set; }

    private int currentCount;

    protected override void OnInitialized()
    {
        currentCount = InitialCount;
    }

    public async Task IncrementCount()
    {
        int newCount = currentCount + 1;
        currentCount = newCount;
        await OnCountChanged.InvokeAsync(currentCount);
    }

    public int CurrentCount { get; }
}