using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

public class SharedCounter : IAsyncLifetime
{
    private int counter;
    private readonly object _lock = new();

    public ValueTask InitializeAsync()
    {
        counter = 0;
        return default;
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }

    public int IncrementAndGet()
    {
        return Interlocked.Increment(ref counter);
    }
}

[assembly: AssemblyFixture(typeof(SharedCounter))]

public class FirstCounterTests(SharedCounter sharedCounter)
{
    [Fact]
    public void IncrementAndGet_ShouldReturnGreaterThanZero()
    {
        int result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void IncrementAndGet_ShouldReturnGreaterThanZero_UsingTestContext()
    {
        SharedCounter sharedCounter = TestContext.Current.GetFixture<SharedCounter>();
        int result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}