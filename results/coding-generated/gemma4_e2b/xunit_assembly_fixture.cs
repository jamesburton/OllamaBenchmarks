public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

    public ValueTask InitializeAsync()
    {
        counter = 0;
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
    }
}

public class FirstCounterTests
{
    private readonly SharedCounter counter;

    public FirstCounterTests()
    {
        counter = new SharedCounter();
    }

    [Fact]
    public void TestIncrementAndGet_ConstructorInjection()
    {
        int result = counter.IncrementAndGet();
        // Since the counter starts at 0, the first increment should yield 1.
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementAndGet_FixtureInjection()
    {
        // Retrieve the fixture from the TestContext
        var counter = TestContext.Current.GetFixture<SharedCounter>();

        int result = counter.IncrementAndGet();
        // Since the counter starts at 0, the first increment should yield 1.
        Assert.True(result > 0);
    }
}