using System;
using Xunit;
using AwesomeAssertions;

[Collection(typeof(CounterCollection))]
public class CounterService
{
    public int Count { get; private set; } = 0;

    public void Increment() => Count++;

    public void Reset() => Count = 0;
}

public class CounterFixture : IAsyncLifetime
{
    public ValueTask InitializeAsync() =>
        Task.FromResult(Counter = new CounterService());

    public ValueTask DisposeAsync() => default;
}

[Collection("Counter")]
public class CounterFirstTests
{
    private readonly CounterFixture _fixture;

    public CounterFirstTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void TestIncrement()
    {
        _fixture.Counter.Increment();
        _fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}

[Collection("Counter")]
public class CounterSecondTests
{
    private readonly CounterFixture _fixture;

    public CounterSecondTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void TestIncrementTwice()
    {
        _fixture.Counter.Increment();
        _fixture.Counter.Increment();
        _fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}

[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
}