public class CounterService
{
    public int Count { get; private set; } = 0;

    public void Increment() => Count++;

    public void Reset() => Count = 0;
}

// CounterFixture implementing IAsyncLifetime with ValueTask returns
public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;

    public ValueTask InitializeAsync() => Task.CompletedValue;

    public ValueTask DisposeAsync() => Task.CompletedValue;
}

// Collection definition marker
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    // No test methods required
}

// Marker class for collection fixture
[CollectionDefinition("Counter")]
public class CounterCollection { }

// Test class 1
public class CounterFirstTests
{
    private readonly CounterFixture _fixture;

    public CounterFirstTests(CounterFixture fixture) => _fixture = fixture;

    [Fact]
    public void IncrementOnce_ShouldIncreaseCount()
    {
        _fixture.Counter.Increment();
        _fixture.Counter.Should().BeGreaterThanOrEqualTo(1);
    }
}

// Test class 2
public class CounterSecondTests
{
    private readonly CounterFixture _fixture;

    public CounterSecondTests(CounterFixture fixture) => _fixture = fixture;

    [Fact]
    public void IncrementTwice_ShouldIncreaseCountToTwo()
    {
        _fixture.Counter.Increment();
        _fixture.Counter.Increment();
        _fixture.Counter.Should().BeGreaterThanOrEqualTo(2);
    }
}