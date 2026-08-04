using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    public int InitialCount { get; set; }
    public EventCallback<int> OnCountChanged { get; set; }

    private int currentCount;

    public void OnInitialized()
    {
        Initialize();
    }

    public async Task IncrementCount()
    {
        await OnCountChanged.InvokeAsync(currentCount);
    }

    public int CurrentCount
    {
        get
        {
            return currentCount;
        }
    }

    private void Initialize()
    {
        this.currentCount = InitialCount;
    }
}