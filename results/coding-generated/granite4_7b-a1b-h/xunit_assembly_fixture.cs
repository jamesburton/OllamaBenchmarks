using System;
using System.Threading;
using Xunit;

[assembly: AssemblyFixture(typeof(SharedCounter))]

public class SharedCounter : IAsyncLifetime
{
    private int counter;

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

public class FirstCounterTests
{
    private readonly SharedCounter _sharedCounter;

    public FirstCounterTests(SharedCounter sharedCounter)
    {
        _sharedCounter = sharedCounter;
    }

    [Fact]
    public void TestIncrementAndGet()
    {
        int result = _sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementAndGetUsingFixture()
    {
        SharedCounter sharedCounter = TestContext.Current.GetFixture<SharedCounter>();
        int result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0);
    }
}