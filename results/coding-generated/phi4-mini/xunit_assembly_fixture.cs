using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

    public ValueTask InitializeAsync() => default;

    public ValueTask DisposeAsync() => default;

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
    }
}

public class FirstCounterTests
{
    [Fact]
    public void TestIncrementedValueIsGreaterThanZero()
    {
        var sharedCounter = new SharedCounter();
        sharedCounter.InitializeAsync().Wait(); // Synchronously wait for async initialization (not recommended in production code)

        int result = sharedCounter.IncrementAndGet();

        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementedValueIsGreaterThanZeroUsingTestContext()
    {
        var testFixture = TestContext.Current.GetFixture<SharedCounter>();
        int result = testFixture.IncrementAndGet();

        Assert.True(result > 0);
    }
}