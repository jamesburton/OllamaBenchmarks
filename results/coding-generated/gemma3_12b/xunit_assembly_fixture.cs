public class SharedCounter : IAsyncLifetime
{
    private int counter;
    private readonly object _lock = new();

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

public class FirstCounterTests(SharedCounter sharedCounter)
{
    [Fact]
    public async Task IncrementAndGet_ShouldBeGreaterThanZero()
    {
        int result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public async Task IncrementAndGet_ShouldBeGreaterThanZero()
    {
        SharedCounter sharedCounter = TestContext.Current.GetFixture<SharedCounter>();
        int result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}