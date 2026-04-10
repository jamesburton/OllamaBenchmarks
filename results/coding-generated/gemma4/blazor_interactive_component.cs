using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

public class CounterBase : ComponentBase
{
    // 2. InitialCount Parameter
    [Parameter]
    public int InitialCount { get; set; }

    // 3. OnCountChanged Parameter
    [Parameter]
    public EventCallback<int> OnCountChanged { get; set; }

    // 4. Private field
    private int currentCount;

    // 6. Lifecycle method initialization
    protected override void OnInitialized()
    {
        // Initialize currentCount using the parameter
        currentCount = InitialCount;
        base.OnInitialized();
    }

    // 6. Public read-only property for testing
    public int CurrentCount => currentCount;

    // 5. Increment method
    public async Task IncrementCount()
    {
        currentCount++;
        // Invoke the event callback
        await OnCountChanged.InvokeAsync(currentCount);
    }
}