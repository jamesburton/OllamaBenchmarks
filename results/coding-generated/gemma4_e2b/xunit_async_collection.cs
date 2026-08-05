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
    public CounterService Counter { get; private set; } = null;

    public ValueTask InitializeAsync()
    {
        Counter = new CounterService();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        // No resources to dispose
        return ValueTask.CompletedTask;
    }
}

[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
}

public class CounterFirstTests : IClassFixture<CounterFixture>
{
    private readonly CounterFixture _fixture;

    public CounterFirstTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Increment_FirstTest_IncrementsCount()
    {
        _fixture.Counter.Increment();

        // Using AwesomeAssertions
        _fixture.Counter.Count.Should().BeGreaterThan(0);
    }
}

public class CounterSecondTests : IClassFixture<CounterFixture>
{
    private readonly CounterFixture _fixture;

    public CounterSecondTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Increment_SecondTest_IncrementsCountCorrectly()
    {
        _fixture.Counter.Increment();
        _fixture.Counter.Increment();

        // Using AwesomeAssertions
        _fixture.Counter.Count.Should().Be(2);
    }
}