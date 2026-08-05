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
    private readonly SharedCounter _counter;

    public FirstCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Fact]
    public void TestIncrementAndGetViaConstructorInjection()
    {
        var result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    private readonly SharedCounter _counter;

    public SecondCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Fact]
    public void TestIncrementAndGetViaTestContext()
    {
        var result = TestContext.Current.GetFixture<SharedCounter>().IncrementAndGet();
        Assert.True(result > 0);
    }
}