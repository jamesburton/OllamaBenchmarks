using AwesomeAssertions;
using Xunit;
using Xunit.Abstractions;

// MARKER CLASS — ICollectionFixture implementation — no test methods
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    // xUnit v3 requires global fixture class for collection definition
    // Declaration only — no tests
}

// Collection fixture — implements IAsyncLifetime returning ValueTask
public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;

    public ValueTask InitializeAsync() => 
        default; // returns completed ValueTask

    public ValueTask DisposeAsync() => 
        default; // returns completed ValueTask
}

// Shared state fixture holder — used via ICollectionFixture attribute
public class CounterFixture : IAsyncLifetime, ICollectionFixture<CounterFixture>
{
    public CounterService Counter { get; private set; } = null!;

    public ValueTask InitializeAsync() => 
        default;

    public ValueTask DisposeAsync() => 
        default;

// Two test classes sharing the CounterFixture via collection
public class CounterFirstTests : IClassFixture<CounterFixture>
{
    private readonly CounterFixture _fixture;
    public CounterFirstTests(CounterFixture fixture) => _fixture = fixture;

    [Fact]
    public void Counter_Is_Initialized_And_Incremented()
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
    public void Counter_Is_Incremented_Twice()
    {
        _fixture.Counter.Increment();
        _fixture.Counter.Increment();
        _fixture.Counter.Count.Should().BeGreaterOrEqualTo(2);
    }
}