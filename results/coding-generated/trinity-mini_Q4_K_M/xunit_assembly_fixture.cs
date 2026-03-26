using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int _counter;

    public ValueTask InitializeAsync() => default;

    public ValueTask DisposeAsync() => default;

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref _counter);
    }
}

[Fact]
public class FirstCounterTests
{
    private readonly SharedCounter _sharedCounter;

    public FirstCounterTests(SharedCounter sharedCounter)
    {
        _sharedCounter = sharedCounter;
    }

    [Fact]
    public void IncrementAndGet_IsGreaterThanZero()
    {
        int result = _sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

[Fact]
public class SecondCounterTests
{
    private readonly SharedCounter _sharedCounter;

    public SecondCounterTests(SharedCounter sharedCounter)
    {
        _sharedCounter = sharedCounter;
    }

    [Fact]
    public void IncrementAndGet_IsGreaterThanZero()
    {
        int result = _sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}
</generated_code>