using AwesomeAssertions;

public class CounterService
{
    public int Count { get; private set; } = 0;
    public void Increment() => Count++;
    public void Reset() => Count = 0;
}

public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;
    public ValueTask InitializeAsync() => ValueTask.Default;
    public ValueTask DisposeAsync() => ValueTask.Default;
}

[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture> { }

[Collection("Counter")]
public class CounterFirstTests
{
    [Fact]
    public void Count_AfterIncrement_ShouldBeAtLeastOne()
    {
        var fixture = TestContext.CurrentFixture as CounterFixture;
        fixture.Counter.Increment();
        Count.Should().BeGreaterOrEqualTo(1);
    }
}

[Collection("Counter")]
public class CounterSecondTests
{
    [Fact]
    public void Count_AfterDoubleIncrement_ShouldBeAtLeastTwo()
    {
        var fixture = TestContext.CurrentFixture as CounterFixture;
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        Count.Should().BeGreaterOrEqualTo(2);
    }
}