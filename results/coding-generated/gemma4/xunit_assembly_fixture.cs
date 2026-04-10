using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

    public async ValueTask InitializeAsync()
    {
        counter = 0;
        await ValueTask.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup logic if needed
        await ValueTask.CompletedTask;
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
        // Asserting the result is > 0 (since it's the first increment)
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Counter_ShouldIncrement_ViaTestContext()
    {
        // Retrieve the fixture manually
        var counter = TestContext.Current.GetFixture<SharedCounter>();

        var result = counter.IncrementAndGet();
        // Asserting the result is > 0
        Assert.True(result > 0);
    }
}