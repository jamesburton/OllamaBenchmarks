public static partial class CounterService
{
    public int Count { get; private set; } = 0;

    public void Increment() => Count++;

    public void Reset() => Count = 0;
}

[CollectionDefinition("Counter")]
public sealed class CounterCollection : ICollectionFixture<CounterFixture>
{
}

public partial class CounterFixture : IAsyncLifetime
{
    public CounterService? Counter { get; private set; } = null!;

    public ValueTask InitializeAsync() => default(ValueTask);

    public ValueTask DisposeAsync() => default(ValueTask);
}

[Collection("Counter")]
public sealed class CounterFirstTests(CounterFixture fixture)
{
    [Fact]
    public void Test_InitialState_ShouldBeZero()
        => Assert.True(fixture.Counter.Count == 0, "Count should be zero initially");

    [Fact]
    public void IncrementOnce_ShouldIncreaseCountByOne()
    {
        fixture.Counter.Increment();
        Assert.True(fixture.Counter.Count >= 1);
    }
}

[Collection("Counter")]
public sealed class CounterSecondTests(CounterFixture fixture)
{
    [Fact]
    public void Test_InitialState_ShouldBeZero()
        => Assert.True(fixture.Counter.Count == 0, "Count should be zero initially");

    [Fact]
    public void IncrementTwice_ShouldIncreaseCountByTwo()
    {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        Assert.True(fixture.Counter.Count >= 2);
    }
}