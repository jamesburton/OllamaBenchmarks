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

    // 4. Private state field
    private int currentCount;

    // 6. Lifecycle method to initialize state
    protected override void OnInitialized()
    {
        currentCount = InitialCount;
    }

    // 6. Public read-only property for testing
    public int CurrentCount => currentCount;

    // 5. Increment method
    public async Task IncrementCount()
    {
        currentCount++;
        // Constraint: Must return async Task
        await OnCountChanged.InvokeAsync(currentCount);
    }
}