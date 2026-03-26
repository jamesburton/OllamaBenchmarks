using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

    public ValueTask InitializeAsync() => new(ValueTaskStatus.Completed);
    public ValueTask DisposeAsync() => new(ValueTaskStatus.Completed);

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
    public void TestIncrementAndGet()
    {
        int result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestContextGetFixture()
    {
        SharedCounter counter = TestContext.Current.GetFixture<SharedCounter>();
        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}