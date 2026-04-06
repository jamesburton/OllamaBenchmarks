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

// CounterFixture.cs
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

// CounterCollection.cs
[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    // This class provides the shared fixture to all tests in the collection
}

// CounterFirstTests.cs
[Collection("Counter")]
public class CounterFirstTests(CounterFixture fixture)
{
    [Fact]
    public void Increment_ShouldIncreaseCount()
    {
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}

// CounterSecondTests.cs
[Collection("Counter")]
public class CounterSecondTests(CounterFixture fixture)
{
    [Fact]
    public void IncrementTwice_ShouldIncreaseCount()
    {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}