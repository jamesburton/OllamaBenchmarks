using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class SharedCounter : IAsyncLifetime
{
    private int _counter;

    public ValueTask InitializeAsync()
    {
        _counter = 0;
        return default;
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }

    public int IncrementAndGet()
    {
        return System.Threading.Interlocked.Increment(ref _counter);
    }
}

[assembly: AssemblyFixture(typeof(SharedCounter))]

public class FirstCounterTests
{
    private readonly SharedCounter _counter;

    public FirstCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Fact]
    public void IncrementAndGet_ReturnsPositiveValue()
    {
        var result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void IncrementAndGet_ReturnsPositiveValue()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        var result = counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}