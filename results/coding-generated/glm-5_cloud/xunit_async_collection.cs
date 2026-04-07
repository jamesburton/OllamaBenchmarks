public class CounterService
{
    public int Count { get; private set; }

    public void Increment()
    {
        Count++;
    }

    public void Reset()
    {
        Count = 0;
    }
}

public class CounterFixture : IAsyncLifetime
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
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    // This class is never instantiated. It serves as the definition for the "Counter" collection.
}

[Collection("Counter")]
public class CounterFirstTests
{
    private readonly CounterFixture _fixture;

    public CounterFirstTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Increment_ShouldIncreaseCount()
    {
        _fixture.Counter.Increment();
        _fixture.Counter.Count.Should().BeGreaterOrEqualTo(1);
    }
}

[Collection("Counter")]
public class CounterSecondTests
{
    private readonly CounterFixture _fixture;

    public CounterSecondTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void IncrementTwice_ShouldIncreaseCount()
    {
        _fixture.Counter.Increment();
        _fixture.Counter.Increment();
        _fixture.Counter.Count.Should().BeGreaterOrEqualTo(2);
    }
}