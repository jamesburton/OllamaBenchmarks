using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int _counter;

    public System.Threading.Tasks.ValueTask InitializeAsync()
    {
        _counter = 0;
        return default;
    }

    public System.Threading.Tasks.ValueTask DisposeAsync() => default;

    public int IncrementAndGet() => Interlocked.Increment(ref _counter);
}

public class FirstCounterTests
{
    private readonly SharedCounter _counter;

    public FirstCounterTests(SharedCounter counter) => _counter = counter;

    [Fact]
    public void Increment_ShouldBePositive()
    {
        var result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Increment_ShouldBePositive()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        var result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}