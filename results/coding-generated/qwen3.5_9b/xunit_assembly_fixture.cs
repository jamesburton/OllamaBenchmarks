public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public ValueTask InitializeAsync()
    {
        this.counter = 0;
        return default;
    }

    public ValueTask DisposeAsync()
    {
        // Cleanup logic if needed
        return default;
    }

    public int IncrementAndGet() => Interlocked.Increment(ref counter);
}

public class FirstCounterTests(SharedCounter sharedCounter)
{
    [Fact]
    public void TestIncrementViaConstructorInjection()
    {
        var result = sharedCounter.IncrementAndGet();
        Assert.True(result > 0, $"Expected value greater than 0, but got: {result}");
    }
}

public class SecondCounterTests(SharedCounter? _unused)
{
    [Fact]
    public void TestIncrementViaTestContext()
    {
        var fixture = TestContext.Current.GetFixture<SharedCounter>();
        Assert.NotNull(fixture); // Ensure the fixture was resolved correctly

        var result = fixture.IncrementAndGet();
        Assert.True(result > 0, $"Expected value greater than 0, but got: {result}");
    }
}