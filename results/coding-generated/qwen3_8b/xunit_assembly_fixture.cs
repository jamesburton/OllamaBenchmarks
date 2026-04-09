using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public ValueTask InitializeAsync()
    {
        counter = 0;
        return default;
    }

    public ValueTask DisposeAsync() => default;

    public int IncrementAndGet() => Interlocked.Increment(ref counter);
}

public class FirstCounterTests
{
    private readonly SharedCounter _counter;

    public FirstCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Fact]
    public void TestIncrement()
    {
        var result = _counter.IncrementAndGet();
        result.Should().BeGreaterThan(0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrement()
    {
        var counter = TestContext.Current.GetFixture<SharedCounter>();
        var result = counter.IncrementAndGet();
        result.Should().BeGreaterThan(0);
    }
}