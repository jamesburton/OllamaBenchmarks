public class SharedCounter : IAsyncLifetime
{
    private int counter;
    public SharedCounter() { }
    public ValueTask InitializeAsync() => ValueTask.FromResult(default);
    public ValueTask DisposeAsync() => ValueTask.FromResult(default);
    public int IncrementAndGet() => Interlocked.Increment(ref counter);
    public int GetCounter() => counter;
}

public class FirstCounterTests
{
    private readonly SharedCounter _counter;
    public FirstCounterTests(SharedCounter counter) => _counter = counter;
    [Fact]
    public void IncrementAndGet_ReturnsGreaterThanZero() => _counter.IncrementAndGet() > 0;
}

public class SecondCounterTests
{
    private readonly SharedCounter _counter;
    public SecondCounterTests() => _counter = SharedCounter.GetFixture();
    [Fact]
    public void IncrementAndGet_ReturnsGreaterThanZero() => _counter.IncrementAndGet() > 0;
}