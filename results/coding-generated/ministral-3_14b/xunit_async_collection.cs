public class CounterService
{
    public int Count { get; private set; }

    public CounterService()
    {
        Count = 0;
    }

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
}

[Collection("Counter")]
public class CounterFirstTests(CounterFixture fixture)
{
    [Fact]
    public void FirstTest_ShouldIncrementCounter()
    {
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterThan(0);
    }
}

[Collection("Counter")]
public class CounterSecondTests(CounterFixture fixture)
{
    [Fact]
    public void SecondTest_ShouldIncrementCounterTwice()
    {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}