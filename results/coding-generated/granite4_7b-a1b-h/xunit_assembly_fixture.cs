using System.Threading;
using Xunit;

public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

    public ValueTask InitializeAsync() => new ValueTask(Task.Run(() => { }));

    public ValueTask DisposeAsync() => new ValueTask();

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
    }
}

public class FirstCounterTests
{
    private readonly SharedCounter _sharedCounter;

    public FirstCounterTests(SharedCounter sharedCounter)
    {
        _sharedCounter = sharedCounter;
    }

    [Fact]
    public void TestIncrementAndGet()
    {
        int result = _sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    private readonly SharedCounter _sharedCounter;

    public SecondCounterTests(SharedCounter sharedCounter)
    {
        _sharedCounter = sharedCounter;
    }

    [Fact]
    public void TestIncrementAndGetWithContext()
    {
        int result = _sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}