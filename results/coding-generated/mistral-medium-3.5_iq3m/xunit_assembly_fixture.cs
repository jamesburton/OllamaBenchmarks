using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public ValueTask InitializeAsync() => default;
    public ValueTask DisposeAsync() => default;

    public int IncrementAndGet() => Interlocked.Increment(ref counter);
}

public class FirstCounterTests(SharedCounter counter)
{
    [Fact]
    public void IncrementAndGet_ReturnsPositiveValue()
    {
        var result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void IncrementAndGet_ReturnsPositiveValue()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        var result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}