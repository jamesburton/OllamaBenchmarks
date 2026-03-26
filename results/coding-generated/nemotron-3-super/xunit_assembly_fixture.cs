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

public class FirstCounterTests
{
    private readonly SharedCounter _counter;
    public FirstCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }
    [Fact]
    public void Increment_ReturnsPositive()
    {
        var value = _counter.IncrementAndGet();
        Assert.True(value > 0);
    }
}

public class SecondCounterTests
{
    public SecondCounterTests()
    {
    }
    [Fact]
    public void Increment_ReturnsPositiveViaTestContext()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        var value = counter.IncrementAndGet();
        Assert.True(value > 0);
    }
}