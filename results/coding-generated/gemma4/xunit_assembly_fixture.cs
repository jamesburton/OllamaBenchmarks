using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

    public async ValueTask InitializeAsync()
    {
        counter = 0;
        await default;
    }

    public async ValueTask DisposeAsync()
    {
        await default;
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
    public void Counter_ShouldIncrement_ViaConstructorInjection()
    {
        var result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Counter_ShouldIncrement_ViaTestContext()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        var result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}