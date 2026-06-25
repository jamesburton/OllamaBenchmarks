[CollectionDefinition("Counter")]
class CounterCollection : ICollectionFixture<CounterFixture>
{
}

class CounterService
{
    public int Count { get; private set; } = 0;
    public void Increment() => Count++;
    public void Reset() => Count = 0;
}

class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;
    public ValueTask InitializeAsync()
    {
        Counter = new CounterService();
        return default;
    }
    public ValueTask DisposeAsync() => default;
}

[Collection("Counter")]
class CounterFirstTests(CounterFixture fixture)
{
    [Fact]
    void Test()
    {
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}

[Collection("Counter")]
class CounterSecondTests(CounterFixture fixture)
{
    [Fact]
    void Test()
    {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}