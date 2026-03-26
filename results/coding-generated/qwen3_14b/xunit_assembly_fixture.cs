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

public class FirstCounterTests(SharedCounter counter)
{
    [Fact]
    public void TestIncrement()
    {
        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrement()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}