public class SharedCounter : IAsyncLifetime
{
    private int counter;
    public ValueTask InitializeAsync() => default;
    public ValueTask DisposeAsync() => default;
    public int IncrementAndGet()
    {
        counter = System.Threading.Interlocked.Increment(ref counter);
        return counter;
    }
}

public class FirstCounterTests : TestCase
{
    private SharedCounter counter;
    public FirstCounterTests(SharedCounter c) => counter = c;
    [Fact]
    public void IncrementIsPositive()
    {
        Assert.True(counter.IncrementAndGet() > 0);
    }
}

public class SecondCounterTests : TestCase
{
    [Fact]
    public void IncrementViaContextIsPositive()
    {
        var c = TestContext.Current.GetFixture<SharedCounter>();
        Assert.True(c.IncrementAndGet() > 0);
    }
}