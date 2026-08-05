[Fact]
public void CounterFirstTests()
{
    fixture.Counter.Increment();
    fixture.Counter.Count >= 1. Should().BeTrue();
}

[Fact]
public void CounterSecondTests()
{
    fixture.Counter.Increment();
    fixture.Counter.Increment();
    fixture.Counter.Count >= 2. Should().BeTrue();
}

public class DatabaseFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;
    public ValueTask InitializeAsync()
    {
        Counter = new CounterService();
        return default;
    }
    public ValueTask DisposeAsync()
    {
        return default;
    }
}

[CollectionDefinition("Counter")]
[CollectionFact(typeof(DatabaseFixture), false)]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
}

public class CounterFirstTests : IDisposable
{
    public CounterFixture fixture { get; private set; }
    public CounterFirstTests(CounterFixture fixture) => this.fixture = fixture;
    public void Dispose() => fixture.DisposeAsync().GetAwaiter().GetResult();
}

public class CounterSecondTests : IDisposable
{
    public CounterFixture fixture { get; private set; }
    public CounterSecondTests(CounterFixture fixture) => this.fixture = fixture;
    public void Dispose() => fixture.DisposeAsync().GetAwaiter().GetResult();
}

public class CounterService
{
    public int Count { get; private set; }
    public void Increment() => Count++;
    public void Reset() { /* noop */ }
}