[assembly: Xunit.AssemblyFixture(typeof(SharedCounter))]

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
    public void TestIncrementAndGet_ShouldReturnGreaterThanZero()
    {
        var result = _sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementAndGet_UsingTestContext_ShouldReturnGreaterThanZero()
    {
        var sharedCounter = TestContext.Current.GetFixture<SharedCounter>();
        var result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}