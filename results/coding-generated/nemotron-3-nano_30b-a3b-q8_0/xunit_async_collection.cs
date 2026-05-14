using AwesomeAssertions;
using Xunit;

public class CounterService
{
    public int Count { get; private set; }
    public void Increment() => Count++;
    public void Reset() => Count = 0;
}

public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;

    public ValueTask InitializeAsync() => Counter = new CounterService();
    public ValueTask DisposeAsync() => default;
}

[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
}

public class CounterFirstTests : IClassFixture<CounterFixture>
{
    private readonly CounterFixture _fixture;

    public CounterFirstTests(CounterFixture fixture) => _fixture = fixture;

    [Fact]
    public void Increment_Operation_Increases_Count()
    {
        _fixture.Counter.Increment();
        _fixture.Counter.Count.Should().BeGreaterOrEqualTo(1);
    }
}

public class CounterSecondTests : IClassFixture<CounterFixture>
{
    private readonly CounterFixture _fixture;

    public CounterSecondTests(CounterFixture fixture) => _fixture = fixture;

    [Fact]
    public void Double_Increment_Operation_Increases_Count_To_At_Least_2()
    {
        _fixture.Counter.Increment();
        _fixture.Counter.Increment();
        _fixture.Counter.Count.Should().BeGreaterOrEqualTo(2);
    }
}