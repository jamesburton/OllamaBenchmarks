using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

    public ValueTask InitializeAsync()
    {
        counter = 0;
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
    }
}

public class FirstCounterTests
{
    private readonly SharedCounter _counter;

    public FirstCounterTests()
    {
        _counter = new SharedCounter();
    }

    [Fact]
    public void IncrementAndGet_shouldReturnGreaterThanZero()
    {
        int result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void IncrementAndGet_viaFixture_shouldReturnGreaterThanZero()
    {
        // Retrieve the fixture using TestContext
        var counter = TestContext.Current.GetFixture<SharedCounter>();

        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}