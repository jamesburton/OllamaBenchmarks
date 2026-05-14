using System.Threading;

// Shared counter fixture implementing IAsyncLifetime
public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public ValueTask InitializeAsync() => default;
    public ValueTask DisposeAsync() => default;

    public int IncrementAndGet() => Interlocked.Increment(ref counter);
}

// Tests using constructor injection
public class FirstCounterTests(SharedCounter counter)
{
    [Fact]
    public void IncrementAndGet_ReturnsPositive()
    {
        var result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

// Tests using TestContext fixture resolution
public class SecondCounterTests
{
    [Fact]
    public void IncrementAndGet_ReturnsPositive()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        var result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}