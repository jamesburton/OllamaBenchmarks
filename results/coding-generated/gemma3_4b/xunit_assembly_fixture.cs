public class SharedCounter
{
    private int counter = 0;

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
    private readonly SharedCounter _counter;

    public FirstCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Fact]
    public void IncrementAndGet_ReturnsGreaterThanZero()
    {
        int result = _counter.IncrementAndGet();
        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    private readonly SharedCounter _counter;

    public SecondCounterTests(SharedCounter counter)
    {
        _counter = counter;
    }

    [Fact]
    public void GetFixture_IncrementAndGet_ReturnsGreaterThanZero()
    {
        var fixture = TestContext.Current.GetFixture<SharedCounter>();
        int result = fixture.IncrementAndGet();
        Assert.True(result > 0);
    }
}