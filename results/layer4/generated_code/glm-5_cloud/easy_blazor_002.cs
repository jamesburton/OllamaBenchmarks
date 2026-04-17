using Microsoft.AspNetCore.Components;

public class CounterComponent : ComponentBase
{
    public int Count { get; private set; } = 0;

    public void Increment()
    {
        Count++;
    }

    public void Decrement()
    {
        if (Count > 0)
        {
            Count--;
        }
    }

    public void Reset()
    {
        Count = 0;
    }
}