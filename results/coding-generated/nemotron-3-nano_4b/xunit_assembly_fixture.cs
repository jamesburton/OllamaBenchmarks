public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;
    public ValueTask InitializeAsync() => default;
    public ValueTask DisposeAsync() => default;
    public int IncrementAndGet()
    {
        int inc = Interlocked.Increment(ref counter);
        return inc;
    }
}
public class FirstCounterTests
{
    [Fact]
    public void IncRunsAndReturnsGreaterThanZero()
    {
        var counter = new SharedCounter();
        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}
[TestClass]
public class SecondCounterTests
{
    [Fact]
    public void IncRunsAndReturnsGreaterThanZero()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        int result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}