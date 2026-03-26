using Microsoft.AspNetCore.Components;

public class CounterBase : Microsoft.AspNetCore.Components.ComponentBase
{
    private int currentCount;
    private void OnInitialized() => currentCount = InitialCount;

    [Parameter] public int InitialCount { get; set; }
    [Parameter] public EventCallback<int> OnCountChanged { get; set; }

    public async Task IncrementCount() => currentCount++; OnCountChanged.InvokeAsync(currentCount);

    public int CurrentCount => currentCount;
}