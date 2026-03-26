using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;
    public System.Threading.Tasks.ValueTask InitializeAsync() => System.Threading.Tasks.ValueTask.Default;
    public System.Threading.Tasks.ValueTask DisposeAsync() => System.Threading.Tasks.ValueTask.Default;
    public int IncrementAndGet() => Interlocked.Increment(ref counter);
}

public class FirstCounterTests
{
    private readonly SharedCounter _counter;
    public FirstCounterTests(SharedCounter counter) => _counter = counter;

    [Fact]
    public void IncrementReturnsPositive()
    {
        int result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void IncrementReturnsPositive()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}