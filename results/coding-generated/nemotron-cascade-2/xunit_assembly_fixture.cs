using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public System.Threading.Tasks.ValueTask InitializeAsync()
    {
        counter = 0;
        return default;
    }

    public System.Threading.Tasks.ValueTask DisposeAsync()
    {
        return default;
    }

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
    }
}

public class FirstCounterTests(SharedCounter counter)
{
    [Fact]
    public void Increment_ReturnsPositive()
    {
        int result = counter.IncrementAndGet();
        Assert.True(result