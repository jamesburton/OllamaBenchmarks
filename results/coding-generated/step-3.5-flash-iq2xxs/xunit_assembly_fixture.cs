public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

    public ValueTask InitializeAsync() => default;

    public ValueTask DisposeAsync() => default;

    public int IncrementAndGet() => Interlocked.Increment(ref counter);
}

public class FirstCounterTests
{
    private readonly SharedCounter _sharedCounter;
    public FirstCounterTests(SharedCounter sharedCounter) => _sharedCounter = sharedCounter;

    [Fact]
    public void Test() => Assert.True(_sharedCounter.IncrementAndGet() > 0);
}

public class SecondCounterTests
{
    [Fact]
    public void Test()
    {
        var sharedCounter = TestContext.Current.GetFixture<SharedCounter>();
        Assert.True(sharedCounter.IncrementAndGet() > 0);
    }
}