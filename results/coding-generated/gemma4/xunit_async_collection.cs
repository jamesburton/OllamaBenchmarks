using Xunit;
using Xunit.Abstractions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

// --- Shared State Service ---
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

// --- Shared Fixture ---
public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Counter = new CounterService();
        await default; // Simulate async initialization
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup logic if necessary
        await default;
    }
}

// --- Collection Definition ---
[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    // This class serves only to define the collection name "Counter"
}

// --- Test Class 1 ---
[Collection("Counter")]
public class CounterFirstTests
{
    private readonly CounterFixture _fixture;

    public CounterFirstTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Test_First_Increment()
    {
        // Action: Increment the shared counter
        _fixture.Counter.Increment();

        // Assertion using AwesomeAssertions
        _fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}

// --- Test Class 2 ---
[Collection("Counter")]
public class CounterSecondTests
{
    private readonly CounterFixture _fixture;

    public CounterSecondTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Test_Second_TwoIncrements()
    {
        // Action: Increment the shared counter twice
        _fixture.Counter.Increment();
        _fixture.Counter.Increment();

        // Assertion using AwesomeAssertions
        // Since CounterFirstTests ran first, the count should be at least 1 + 2 = 3
        _fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}