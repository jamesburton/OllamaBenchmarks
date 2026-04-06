using System.Threading;
using Xunit;

public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public ValueTask InitializeAsync()
    {
        counter = 0;
        return default;
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
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
    public void IncrementAndGet_Returns_GreaterThanZero()
    {
        int result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void IncrementAndGet_Returns_GreaterThanZero_Using_TestContext()
    {
        var fixture = TestContext.Current.GetFixture<SharedCounter>();
        int result = fixture.IncrementAndGet();
        Assert.True(result > 0);
    }
}