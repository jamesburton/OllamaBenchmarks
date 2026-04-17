using System;
using System.Threading.Tasks;
using Xunit;

public class AsyncCounter : IAsyncLifetime
{
    public int Value { get; private set; }

    public ValueTask InitializeAsync()
    {
        Value = 10;
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Value = 0;
        return ValueTask.CompletedTask;
    }

    public void Increment()
    {
        Value += 1;
    }
}