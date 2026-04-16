using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    [Parameter] public int InitialCount { get; set; }

    [Parameter] public EventCallback<int> OnCountChanged { get; set; }

    private int currentCount;

    protected override async Task OnInitializedAsync()
    {
        currentCount = InitialCount;
        await OnCountChanged.InvokeAsync(currentCount);
    }

    public async Task IncrementCount()
    {
        currentCount++;
        await OnCountChanged.InvokeAsync(currentCount);
    }

    public int CurrentCount => currentCount;
}