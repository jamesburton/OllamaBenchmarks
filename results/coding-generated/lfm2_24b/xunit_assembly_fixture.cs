using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;
    public ValueTask InitializeAsync() { counter = 0; return default; }
    public ValueTask DisposeAsync() { return default; }
    public int IncrementAndGet() => Interlocked.Increment(ref counter);
}

public class FirstCounterTests
{
    private readonly SharedCounter counter;
    public FirstCounterTests(SharedCounter counter) => this.counter = counter;
    [Fact]
    public void IncrementAndGet_ReturnsPositive() => Assert.True(counter.IncrementAndGet() > 0);
}

public class SecondCounterTests
{
    private readonly SharedCounter counter;
    public SecondCounterTests()
    {
        var fixture = TestContext.Current.GetFixture<SharedCounter>();
        counter = fixture;
    }
    [Fact]
    public void IncrementAndGet_ReturnsPositive() => Assert.True(counter.IncrementAndGet() > 0);
}