using System;
using System.Collections.Generic;
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

// 3. Marker class implementing ICollectionFixture
[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    // No test methods required
}

// 4. Test Class 1
[Collection("Counter")]
public class CounterFirstTests
{
    public CounterFixture Fixture { get; }

    public CounterFirstTests(CounterFixture fixture)
    {
        Fixture = fixture;
    }

    [Fact]
    public void Increment_FirstTest_IncrementsCount()
    {
        Fixture.Counter.Increment();
        Fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}

// 4. Test Class 2
[Collection("Counter")]
public class CounterSecondTests
{
    public CounterFixture Fixture { get; }

    public CounterSecondTests(CounterFixture fixture)
    {
        Fixture = fixture;
    }

    [Fact]
    public void Increment_SecondTest_IncrementsCountCorrectly()
    {
        Fixture.Counter.Increment();
        Fixture.Counter.Increment();
        Fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}