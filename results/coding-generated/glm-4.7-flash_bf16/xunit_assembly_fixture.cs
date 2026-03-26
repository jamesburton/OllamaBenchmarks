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
    public void TestIncrementViaConstructor()
    {
        _sharedCounter.IncrementAndGet();
        Assert.True(_sharedCounter.Counter > 0便, "Counter should be greater than 0");
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementViaTestContext()
    {
        var sharedCounter = TestContext.Current.GetFixture<SharedCounter>();
        sharedCounter.IncrementAndGet();
        Assert.True(sharedCounter.Counter > 0, "Counter should be greater than 0");
    }
}