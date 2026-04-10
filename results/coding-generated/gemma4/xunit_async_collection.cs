using Xunit;
using Xunit.Abstractions;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

// --- 1. CounterService ---
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

// --- 2. CounterFixture ---
public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Counter = new CounterService();
        await Task.CompletedTask; // Placeholder for async initialization
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup logic if needed
        await Task.CompletedTask;
    }
}

// --- 3. CounterCollection ---
[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    // This class serves only as a collection definition marker.
}

// --- 4. CounterFirstTests ---
[Collection("Counter")]
public class CounterFirstTests
{
    private readonly CounterFixture _fixture;

    public CounterFirstTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void FirstTest_IncrementsCounter()
    {
        // Arrange: Initial state is 0 (set by fixture)

        // Act
        _fixture.Counter.Increment();

        // Assert: Check if the count is at least 1
        _fixture.Counter.Count.Should().BeGreaterOrEqualTo(1);
    }
}

// --- 5. CounterSecondTests ---
[Collection("Counter")]
public class CounterSecondTests
{
    private readonly CounterFixture _fixture;

    public CounterSecondTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void SecondTest_IncrementsCounterTwice()
    {
        // Arrange: The counter state is shared and should be 1 from CounterFirstTests

        // Act
        _fixture.Counter.Increment();
        _fixture.Counter.Increment();

        // Assert: Check if the count is at least 3 (1 initial + 2 increments)
        _fixture.Counter.Count.Should().BeGreaterOrEqualTo(3);
    }
}