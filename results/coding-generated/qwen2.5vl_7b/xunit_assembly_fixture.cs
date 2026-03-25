using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

// SharedCounter class implementing IAsyncLifetime with ValueTask return types
public class SharedCounter : IAsyncLifetime
{
    private int _counter;

    public SharedCounter()
    {
        _counter = 0;
    }

    public ValueTask InitializeAsync() => new ValueTask(Task.CompletedTask);

    public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);

    public int IncrementAndGet()
    {
        Interlocked.Increment(ref _counter);
        return _counter;
    }
}

// Assembly-scoped fixture registration
[assembly: AssemblyFixture(typeof(SharedCounter))]

// FirstCounterTests class with constructor injection
public class FirstCounterTests
{
    private readonly SharedCounter _counter;

    public FirstCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Fact]
    public void TestIncrementAndGet()
    {
        var result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

// SecondCounterTests class using TestContext.Current.GetFixture<SharedCounter>()
public class SecondCounterTests
{
    private readonly SharedCounter _counter;

    public SecondCounterTests()
    {
        _counter = TestContext.Current.GetFixture<SharedCounter>();
    }

    [Fact]
    public void TestIncrementAndGet()
    {
        var result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}