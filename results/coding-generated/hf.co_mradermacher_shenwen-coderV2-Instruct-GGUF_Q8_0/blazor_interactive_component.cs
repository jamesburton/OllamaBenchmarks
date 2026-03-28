using Microsoft.AspNetCore.Components;

public class Counter : ComponentBase
{
    private int currentCount = 0;
    [Parameter] public int InitialCount { get; set; } = 0;
    [Parameter] public EventCallback<int> OnCountChanged { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await OnCountChanged.InvokeAsync(InitialCount);
    }

    private async Task IncrementCount()
    {
        currentCount++;
        await OnCountChanged.InvokeAsync(currentCount);
    }
}