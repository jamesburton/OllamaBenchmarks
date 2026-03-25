using Microsoft.AspNetCore.Components;

namespace MyApp.Components;

public class CounterBase : ComponentBase
{
    [Parameter]
    public int InitialCount { get; set; }

    [Parameter]
    public EventCallback<int> OnCountChanged { get; set; }

    private int currentCount;

    public int CurrentCount => currentCount;

    protected override void OnInitialized()
    {
        currentCount = InitialCount;
    }

    public void IncrementCount()
    {
        currentCount++;
        OnCountChanged.InvokeAsync(currentCount);
    }
}