using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public int Counter => counter;

    public ValueTask InitializeAsync()
    {
        counter = 0;
        return ValueTask.FromResult(default);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.FromResult(default);
    }

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
    }
}

using Xunit;
using System.Threading;

public class FirstCounterTests
{
    private readonly SharedCounter _counter;

    public FirstCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Fact]
    public void Increment_ReturnsPositiveValue()
    {
        var value = _counter.IncrementAndGet();
        Assert.True(value > 0);
    }
}

using Xunit;
using System.Threading;

public class SecondCounterTests
{
    private readonly SharedCounter _counter;

    public SecondCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Fact]
    public void Increment_ReturnsPositiveValue()
    {
        var value = _counter.IncrementAndGet();
        Assert.True(value > 0);
    }
}