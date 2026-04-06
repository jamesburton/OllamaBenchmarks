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
    private readonly SharedCounter _counter;
    public FirstCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }
    [Fact]
    public void TestIncrement()
    {
        int value = _counter.IncrementAndGet();
        Assert.True(value > 0);
    }
}

public class SecondCounterTests
{
    public SecondCounterTests() { }
    [Fact]
    public void TestIncrementViaTestContext()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        int value = counter.IncrementAndGet();
        Assert.True(value > 0);
    }
}