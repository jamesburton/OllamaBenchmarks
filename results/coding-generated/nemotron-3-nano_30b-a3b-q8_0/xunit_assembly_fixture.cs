using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;
    public ValueTask InitializeAsync() => default;
    public ValueTask DisposeAsync() => default;
    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
    }
}

public class FirstCounterTests(SharedCounter sharedCounter)
{
    [Fact]
    public void IncrementReturnsPositive()
    {
        var result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void IncrementReturnsPositive()
    {
        var sharedCounter = TestContext.Current.GetFixture<SharedCounter>();
        var result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}