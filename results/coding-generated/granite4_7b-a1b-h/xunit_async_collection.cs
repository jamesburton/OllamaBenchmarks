using Xunit;
using AwesomeAssertions;

public class CounterService
{
    public int Count { get; private set; } = 0;

    public void Increment() => Count++;

    public void Reset() => Count = 0;
}

public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;

    public ValueTask InitializeAsync() => ValueTask.FromResult(Counter = new CounterService());

    public ValueTask DisposeAsync() => ValueTask.FromResult();
}

[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture> { }

public class CounterFirstTests
{
    private readonly CounterFixture fixture;

    public CounterFirstTests(CounterFixture fixture) => this.fixture = fixture;

    [Fact]
    public void Increment_ShouldIncreaseCount()
    {
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterOrEqualTo(1);
    }
}

public class CounterSecondTests
{
    private readonly CounterFixture fixture;

    public CounterSecondTests(CounterFixture fixture) => this.fixture = fixture;

    [Fact]
    public void IncrementTwice_ShouldIncreaseCount()
    {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterOrEqualTo(2);
    }
}