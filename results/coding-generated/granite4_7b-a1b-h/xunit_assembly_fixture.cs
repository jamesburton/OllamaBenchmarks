using System.Threading;
using Xunit;

public class SharedCounter
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
        Assert.True(_sharedCounter.IncrementAndGet() > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementAndGetUsingContext()
    {
        var sharedCounter = TestContext.Current.GetFixture<SharedCounter>();
        Assert.True(sharedCounter.IncrementAndGet() > 0);
    }
}