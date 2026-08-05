using System.Threading;
using Xunit;

public class SharedCounter
{
    private int counter;
    public ValueTask InitializeAsync() => default;
    public ValueTask DisposeAsync() => default;
    public int IncrementAndGet() => Interlocked.Increment(ref counter);

    public int this[int index] { get; set; }
}

public class FirstCounterTests
{
    private SharedCounter counter;

    public FirstCounterTests(SharedCounter counter) => this.counter = counter;

    [Fact]
    public void IncrementAndGet_ReturnsPositiveNumber()
    {
        Assert.True(counter.IncrementAndGet() > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void IncrementAndGet_UsingTestContext_InheritsCounter()
    {
        var counter = Xunit.TestContext.Current.GetFixture<SharedCounter>();
        Assert.True(counter.IncrementAndGet() > 0);
    }
}