using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using AwesomeAssertions;

// 1. CounterService class
public class CounterService
{
    public int Count { get; private set; } = 0;
    public void Increment() => Count++;
    public void Reset() => Count = 0;
}

// 2. CounterFixture class implementing IAsyncLifetime
public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null;

    public ValueTask InitializeAsync()
    {
        Counter = new CounterService();
        return default;
    }

    public ValueTask DisposeAsync()
    {
        // No cleanup needed for this simple example
        return default;
    }
}

// 3. Marker class for collection fixture
[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    // No test methods required
}

// 4. Test Class 1
[Collection("Counter")]
public class CounterFirstTests
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
        _fixture.Counter.Count.Should().Be(1);
    }
}

// 4. Test Class 2
[Collection("Counter")]
public class CounterSecondTests
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
        _fixture.Counter.Count.Should().Be(2);
    }
}