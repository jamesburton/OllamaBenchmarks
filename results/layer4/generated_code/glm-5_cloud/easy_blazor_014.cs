using Microsoft.AspNetCore.Components;

public class CounterComponent : ComponentBase
{
    [Parameter]
    public int InitialCount { get; set; } = 0;

    protected int Count { get; set; }

    protected override void OnInitialized()
    {
        Count = InitialCount;
    }

    public void OnInitializedPublic()
    {
        OnInitialized();
        Count = InitialCount;
    }

    public void Increment()
    {
        Count++;
    }

    public void Decrement()
    {
        Count--;
    }
}