using Xunit;
using Xunit.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;

// 1. CounterService
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

// 2. CounterFixture
public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Counter = new CounterService();
        // Simulate async setup
        await Task.Delay(1); 
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup logic
        await Task.Delay(1);
    }
}

// 3. Marker class for the collection
[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    // This class serves only as a marker for the collection definition.
}

// 4. First Test Class
[Collection("Counter")]
public class CounterFirstTests
{
    private readonly CounterFixture _fixture;

    public CounterFirstTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Test_IncrementsCounterOnce()
    {
        // Arrange: Ensure a clean start for this test run (though the fixture handles setup)
        _fixture.Counter.Reset();

        // Act
        _fixture.Counter.Increment();

        // Assert
        _fixture.Counter.Count.Should().Be(1);
    }
}

// 5. Second Test Class
[Collection("Counter")]
public class CounterSecondTests
{
    private readonly CounterFixture _fixture;

    public CounterSecondTests(CounterFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public void Test_IncrementsCounterTwice()
    {
        // Arrange: The counter state is shared. We must reset it manually 
        // if we want this test to be isolated from the previous test run.
        _fixture.Counter.Reset();

        // Act
        _fixture.Counter.Increment();
        _fixture.Counter.Increment();

        // Assert
        // Because the fixture is shared, this test relies on the state being reset 
        // before the test method runs, ensuring the count is 2.
        _fixture.Counter.Count.Should().Be(2);
    }
}