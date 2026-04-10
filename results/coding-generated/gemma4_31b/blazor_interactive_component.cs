using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

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

    public async Task IncrementCount()
    {
        currentCount++;
        await OnCountChanged.InvokeAsync(currentCount);
    }
}