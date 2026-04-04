using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public ValueTask InitializeAsync() => ValueTask.FromResult(default);

    public ValueTask DisposeAsync() => ValueTask.FromResult(default);

    public int IncrementAndGet()
    {
        Interlocked.Increment(ref counter);
        return counter;
    }
}

public class FirstCounterTests
{
    private readonly SharedCounter _counter;

    public FirstCounterTests()
    {
        _counter = new SharedCounter();
    }

    [Fact]
    public void IncrementAndGet_ReturnsGreaterThanZero()
    {
        var result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    private SharedCounter _counter;

    public SecondCounterTests()
    {
        _counter = SharedCounter.GetFixture();
    }

    [Fact]
    public void IncrementAndGet_ReturnsGreaterThanZero()
    {
        var result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}