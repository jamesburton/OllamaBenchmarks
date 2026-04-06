using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;
    public ValueTask InitializeAsync() => default;
    public ValueTask DisposeAsync() => default;
    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
    }
}

[Fact]
public void FirstCounter_IncrementIsPositive()
{
    var counter = new SharedCounter();
    int value = counter.IncrementAndGet();
    AwesomeAssertions.True(value > 0);
}

[Fact]
public void SecondCounter_IncrementIsPositive()
{
    var sc = TestContext.Current.GetFixture<SharedCounter>();
    int value = sc.IncrementAndGet();
    AwesomeAssertions.True(value > 0);
    TestContext.Current.CancellationToken.CancelAsync();
}