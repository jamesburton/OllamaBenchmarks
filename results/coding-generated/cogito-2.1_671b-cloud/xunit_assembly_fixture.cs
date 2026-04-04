public class SharedCounter : IAsyncLifetime
{
    private int _counter;

    public ValueTask InitializeAsync()
    {
        _counter = 0;
        return default;
    }

    public ValueTask DisposeAsync() => default;

    public int IncrementAndGet() => Interlocked.Increment(ref _counter);
}

public class FirstCounterTests(SharedCounter counter)
{
    [Fact]
    public void Increment_ReturnsPositive()
    {
        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Increment_ReturnsPositive()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}