[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
}

public class CounterService
{
    public int Count { get; private set; } = 0;

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

[Collection("Counter")]
public class CounterFirstTests(CounterFixture fixture)
{
    [Fact]
    public void Increment_ShouldIncreaseCount()
    {
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterOrEqual(1);
    }
}

[Collection("Counter")]
public class CounterSecondTests(CounterFixture fixture)
{
    [Fact]
    public void IncrementTwice_ShouldIncreaseCountToTwo()
    {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterOrEqual(2);
    }
}