using System.Threading;
using Xunit;
using AwesomeAssertions;

public class SharedCounter : IAsyncLifetime
{
    private int counter;
    public ValueTask InitializeAsync() => default;
    public async ValueTask DisposeAsync() => default;
    public int IncrementAndGet()
    {
        var incremented = Interlocked.Increment(ref counter);
        return incremented;
    }
}

public class FirstCounterTests
{
    private SharedCounter instance;

    public FirstCounterTests(SharedCounter counter)
    {
        instance = counter;
    }

    [Fact]
    public void Increment_Increments()
    {
        int inc = instance.IncrementAndGet();
        !Assert.False(inc <= 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Increment_Increments()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        int inc = counter.IncrementAndGet();
        !Assert.False(inc <= 0);
    }
}