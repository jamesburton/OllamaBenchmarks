using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    [Parameter] public int InitialCount { get; set; } = 0;
    [Parameter] public EventCallback<int> OnCountChanged { get; set; }

    private int _currentCount;

    protected override void OnInitialized()
    {
        _currentCount = InitialCount;
    }

    public async Task IncrementCount()
    {
        await StateHasChangedAsync();
        _currentCount++;
        if (OnCountChanged != null)
            await OnCountChanged.InvokeAsync(_currentCount);
    }

    public int CurrentCount
    {
        get { return _currentCount; }
    }
}