using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int _counter;

    public ValueTask InitializeAsync()
    {
        _            counter = 0;
        return default;
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref _counter);
    }
}

public class FirstCounterTests(SharedCounter counter)
{
    [Fact]
    public void Test()
    {
        Assert.True(counter.IncrementAndGet() > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Test()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        Assert.True(counter.IncrementAndGet() > 0);
    }
}