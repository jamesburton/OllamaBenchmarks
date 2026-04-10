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
    public void Test_ConstructorInjection_IncrementsCounter()
    {
        var result = _counter.IncrementAndGet();
        // Assert that the counter was successfully incremented (i.e., result > 0)
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Test_GetFixture_IncrementsCounter()
    {
        // Manually retrieve the fixture instead of using constructor injection
        var counter = TestContext.Current.GetFixture<SharedCounter>();

        var result = counter.IncrementAndGet();
        // Assert that the counter was successfully incremented (i.e., result > 0)
        Assert.True(result > 0);
    }
}