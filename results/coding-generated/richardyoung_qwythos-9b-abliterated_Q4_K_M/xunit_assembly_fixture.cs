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

public class FirstCounterTests(SharedCounter shared)
{
    [Fact]
    public void Test()
    {
        Assert.True(shared.IncrementAndGet() > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public async Task Test()
    {
        var fixture = TestContext.Current.GetFixture<SharedCounter>();
        Assert.True(fixture.IncrementAndGet() > 0);
    }
}