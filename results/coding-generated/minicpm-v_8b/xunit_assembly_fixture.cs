using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

    public ValueTask InitializeAsync()
    {
        Interlocked.Increment(ref counter);
        return default;
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }

    public int IncrementAndGet()
    {
        return Interlocked.Add(ref counter, 1);
    }
}

public class FirstCounterTests
{
    [Fact]
    public void Test()
    {
        var sharedCounter = new SharedCounter();
        Assert.Greater(sharedCounter.IncrementAndGet(), 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Test()
    {
        using (var fixture = TestContext.Current.GetFixture<SharedCounter>())
        {
            Assert.Greater(fixture.IncrementAndGet(), 0);
        }
    }
}