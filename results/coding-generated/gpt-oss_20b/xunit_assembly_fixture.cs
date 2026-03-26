using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public ValueTask InitializeAsync()
    {
        counter = 0;
        return default;
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
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
    public void Increment_ShouldBeGreaterThanZero()
    {
        var result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Increment_ShouldBeGreaterThanZero()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        var result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}