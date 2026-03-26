[assembly: AssemblyFixture(typeof(SharedCounter))]

public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public ValueTask InitializeAsync()
    {
        counter = 0;
        return default;
    }

    public ValueTask DisposeAsync() => default;

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
    }
}

public class FirstCounterTests
{
    private readonly SharedCounter counter;

    public FirstCounterTests(SharedCounter counter)
    {
        this.counter = counter;
    }

    [Fact]
    public void IncrementAndGet_ReturnsPositive()
    {
        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void IncrementAndGet_ReturnsPositive()
    {
        SharedCounter counter = TestContext.Current.GetFixture<SharedCounter>();
        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}