using System.Threading;

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
    private readonly SharedCounter _sharedCounter;

    public FirstCounterTests(SharedCounter sharedCounter)
    {
        _sharedCounter = sharedCounter;
    }

    [Fact]
    public void TestIncrementAndGet()
    {
        var result = _sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementAndGetUsingTestContext()
    {
        var sharedCounter = TestContext.Current.GetFixture<SharedCounter>();
        var result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}