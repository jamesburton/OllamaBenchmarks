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

public class FirstCounterTests(SharedCounter shared)
{
    [Fact]
    public void Test()
    {
        Assert.True(shared.IncrementAndGet() > 0);
    }
}

public class SecondCounterTests
    (TestContext.Current.GetFixture<SharedCounter> shared)
{
    [Fact]
    public void Test()
    {
        Assert.True(shared.IncrementAndGet() > 0);
    }
}