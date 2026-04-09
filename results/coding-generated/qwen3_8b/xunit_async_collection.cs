public class CounterService
{
    public int Count { get; private set; } = 0;
    public void Increment() => Count++;
    public void Reset() => Count = 0;
}

public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;

    public ValueTask InitializeAsync()
    {
        Counter = new CounterService();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    // No test methods
}

public class CounterFirstTests
{
    private readonly CounterFixture fixture;

    public CounterFirstTests(CounterFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void TestIncrement()
    {
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}

public class CounterSecondTests
{
    private readonly CounterFixture fixture;

    public CounterSecondTests(CounterFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void TestIncrementTwice()
    {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}