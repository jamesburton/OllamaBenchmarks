using System.Threading;

class SharedCounter : IAsyncLifetime
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

public class FirstCounterTests
{
    private readonly SharedCounter _counter;
    public FirstCounterTests(SharedCounter counter) => _counter = counter;
    [Fact]
    public void TestIncrement()
    {
        var value = _counter.IncrementAndGet();
        Assert.True(value > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementViaContext()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        var value = counter.IncrementAndGet();
        Assert.True(value > 0);
    }
}