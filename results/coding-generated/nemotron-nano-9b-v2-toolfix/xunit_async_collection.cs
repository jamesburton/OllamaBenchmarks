[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture> { }

public class CounterService
{
    public int Count { get; private set; } = 0;
    public void Increment() => Count++;
    public void Reset() => Count = 0;
}

public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;
    public ValueTask InitializeAsync() => Counter = new CounterService();
    public ValueTask DisposeAsync() => default;
}

[Collection("Counter")]
public class CounterFirstTests
{
    public CounterFirstTests(CounterFixture fixture) { }

    [Fact]
    public void IncrementOnce() => fixture.Counter.Increment().Should().Be(1);
}

[Collection("Counter")]
public class CounterSecondTests
{
    public CounterSecondTests(CounterFixture fixture) { }

    [Fact]
    public void IncrementTwice() => fixture.Counter.Increment().Increment().Should().Be(2);
}