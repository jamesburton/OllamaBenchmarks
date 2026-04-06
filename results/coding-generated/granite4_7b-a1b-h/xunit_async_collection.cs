using Xunit;
using AwesomeAssertions;

public class CounterService
{
    public int Count { get; private set; } = 0;

    public void Increment() => Count++;

    public void Reset() => Count = 0;
}

[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture> { }

public class CounterFixture : IAsyncLifetime
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
        fixture.Counter.Count.Should().BeGreaterOrEqualTo(1);
    }
}

[Collection("Counter")]
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
        fixture.Counter.Count.Should().BeGreaterOrEqualTo(2);
    }
}