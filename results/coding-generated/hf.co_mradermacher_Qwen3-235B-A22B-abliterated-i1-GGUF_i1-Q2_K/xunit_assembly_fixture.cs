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

public class FirstCounterTests(SharedCounter sharedCounter)
{
    [Fact]
    public void TestIncrement()
    {
        int result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementViaTestContext()
    {
        SharedCounter sharedCounter = TestContext.Current.GetFixture<SharedCounter>();
        int result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}