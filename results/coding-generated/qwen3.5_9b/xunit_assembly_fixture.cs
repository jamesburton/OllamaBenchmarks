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

public class FirstCounterTests(SharedCounter sharedCounter)
{
    [Fact]
    public void TestIncrement()
    {
        Assert.True(sharedCounter.IncrementAndGet() > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public async Task TestIncrementWithTestContext()
    {
        var fixture = TestContext.Current.GetFixture<SharedCounter>();
        Assert.True(fixture.IncrementAndGet() > 0);
    }
}