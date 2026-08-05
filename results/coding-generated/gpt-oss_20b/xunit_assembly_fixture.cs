using System.Threading;

public class SharedCounter : Xunit.IAsyncLifetime
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

public class FirstCounterTests
{
    private readonly SharedCounter _counter;

    public FirstCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Xunit.Fact]
    public void Increment_ShouldBeGreaterThanZero()
    {
        int result = _counter.IncrementAndGet();
        Xunit.Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Xunit.Fact]
    public void Increment_ShouldBeGreaterThanZero()
    {
        var counter = Xunit.TestContext.Current.GetFixture<SharedCounter>();
        int result = counter.IncrementAndGet();
        Xunit.Assert.True(result > 0);
    }
}