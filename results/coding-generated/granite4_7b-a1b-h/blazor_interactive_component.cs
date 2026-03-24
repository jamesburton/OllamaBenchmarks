using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

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

    public void IncrementCount()
    {
        currentCount++;
        OnCountChanged.InvokeAsync(currentCount);
    }

    public int CurrentCount => currentCount;
}