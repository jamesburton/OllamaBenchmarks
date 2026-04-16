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

public class FirstCounterTests(SharedCounter counter)
{
    [Fact]
    public void Test()
    {
        Assert.Greater(counter.IncrementAndGet(), 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Test()
    {
        var fixture = TestContext.Current.GetFixture<SharedCounter>();
        Assert.Greater(fixture.IncrementAndGet(), 0);
    }
}