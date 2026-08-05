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
    public void TestCounter()
    {
        Assert.Greater(_counter.IncrementAndGet(), 0);
    }
}

public class SecondCounterTests
{
    private readonly SharedCounter _counter;

    public SecondCounterTests()
    {
        _counter = TestContext.Current.GetFixture<SharedCounter>();
    }

    [Fact]
    public void TestCounter()
    {
        Assert.Greater(_counter.IncrementAndGet(), 0);
    }
}