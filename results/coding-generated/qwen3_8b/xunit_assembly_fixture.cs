using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public ValueTask InitializeAsync() => default;
    public ValueTask DisposeAsync() => default;

    public int IncrementAndGet()
    {
        int current = counter;
        Interlocked.Increment(ref counter);
        return current;
    }
}

public class FirstCounterTests
{
    private readonly SharedCounter _counter;

    public FirstCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Fact]
    public void IncrementAndGet_ReturnsValueGreaterThanZero()
    {
        var result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void IncrementAndGet_ReturnsValueGreaterThanZero()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        var result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}