using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int _counter;

    public ValueTask InitializeAsync()
    {
        _counter = 0;
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
    public void TestIncrement()
    {
        Assert.True(counter.IncrementAndGet() > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementWithContext()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        Assert.True(counter.IncrementAndGet() > 0);
    }
}