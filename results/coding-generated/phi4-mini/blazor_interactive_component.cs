using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

public class CounterBase : ComponentBase
{
    [Parameter] public int InitialCount { get; set; }

    [Parameter] public EventCallback<int> OnCountChanged { get; set; }

    private int currentCount = 0;

    protected override void OnInitialized()
    {
        currentCount = InitialCount;
    }

    public async Task IncrementCount()
    {
        currentCount++;
        await OnCountChanged.InvokeAsync(currentCount);
    }

    public int CurrentCount => currentCount;
}