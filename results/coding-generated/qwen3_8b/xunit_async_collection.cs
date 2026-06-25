[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
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

public class CounterFirstTests([Collection("Counter")] CounterFixture fixture)
{
    [Fact]
    public void Should_Increment_Count()
    {
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}

public class CounterSecondTests([Collection("Counter")] CounterFixture fixture)
{
    [Fact]
    public void Should_Increment_Count_Twice()
    {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}