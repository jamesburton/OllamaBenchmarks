using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int _counter;

    public ValueTask InitializeAsync()
    {
        _counter = 0;
        return default;
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref _counter);
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

public class SecondCounterTests(SharedCounter counter)
{
    [Fact]
    public void TestIncrementViaContext()
    {
        var fixture = TestContext.Current.GetFixture<SharedCounter>();
        int result = fixture.IncrementAndGet();
        Assert.True(result > 0);
    }
}