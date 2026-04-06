[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
    public int Count { get; private set; }
    public void Increment() => Count++;
    public void Reset() => Count = 0;
}

[Collection("Counter")]
[Fact]
public class CounterFirstTests
{
    [Fact]
    public void TestFirst()
    {
        var fixture = TestContext.Current.GetFixture<CounterFixture>();
        fixture.Counter.Increment();
        Assert.Equal(1, fixture.Counter.Count);
    }
}

[Fact]
public class CounterSecondTests
{
    [Fact]
    public void TestSecond()
    {
        var fixture = TestContext.Current.GetFixture<CounterFixture>();
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        Assert.Equal(2, fixture.Counter.Count);
    }
}