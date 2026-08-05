public interface ICollectionFixture<CounterFixture>
{
    // default method signature maybe void?
    // In xUnit v3, CollectionFixture has a constructor that takes the fixture.
    // Actually they use attribute [Collection("Counter")] and then test classes receive via static methods:
    // var fixture = TestContext.Current.GetFixture<DatabaseFixture>();
    // For custom fixtures, they might use [CollectionDefinition] on class and implement ICollectionFixture<T> which provides a method like Get() or something? Let's check xUnit v3 docs.


[Collection("MyCollection")]
public class MyCollectionTests <IMyCollectionFixture>
{
    private static MyFixture fixture;
    //...
    public void Setup()
    {
        fixture = new MyFixture();
        fixture.Initialize();
    }
    public void Test()
    {
        var result = fixture.Get();
    }
}

public interface ICollectionFixture<CounterFixture>
{
    // maybe a method Get() that returns the fixture instance.
    public CounterFixture Get();
}

public interface ICollectionFixture<T> : IDataFixup
{
    // maybe a method to get the fixture instance?
}

public interface ICollectionFixture
{
    void Setup();
    void Test();
}

[CollectionDefinition("MyCollection")]
public class MyCollectionTests <IMyCollectionFixture>
{
    private static MyFixture fixture;
    //...
}

public interface ICollectionFixture<T>
{
    void Setup();
    void Clean();
}

public interface ICollectionFixture<CounterFixture>
{
}

public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!

    public ValueTask InitializeAsync() => ValueTask defaults returning default;
    public ValueTask DisposeAsync() => ValueTask defaults returning default;
}

public class CounterFirstTests
{
    [Fact]
    public void TestCounterIncrement()
    {
        var fixture = new CounterFixture();
        fixture.InitializeAsync(); // maybe not needed but ensures Counter is assigned?
        // Actually Counter already assigned to new instance in constructor? No, we set Counter to null! and InitializeAsync assigns it. So need to call InitializeAsync.
        fixture.Counter.Increment();
        Assert.Equal(1, fixture.Counter.Count);
    }
}

Assert.Equal(1, fixture.Counter.Count);

public class CounterSecondTests
{
    [Fact]
    public void TestCounterIncrementTwice()
    {
        var fixture = new CounterFixture();
        fixture.InitializeAsync();
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        Assert.Equal(2, fixture.Counter.Count);
    }
}

public class CounterFirstTests
{
    public CounterFixture fixture;
    [Fact]
    public void TestCounterIncrement()
    {
        fixture.Counter.Increment();
        Assert.Equal(1, fixture.Counter.Count);
    }
}

public class CounterSecondTests
{
    public CounterFixture fixture;
    [Fact]
    public void TestCounterIncrementTwice()
    {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        Assert.Equal(2, fixture.Counter.Count);
    }
}

[Fact]
public void TestCounterIncrement()
{
    fixture.InitializeAsync();
    fixture.Counter.Increment();
    Assert.Equal(1, fixture.Counter.Count);
}

public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!

    public ValueTask InitializeAsync()
    {
        Counter = new CounterService();
        return ValueTask defaults; // or default
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask defaults;
    }
}

public class MyFixture : IAsyncLifetime
{
    public MyService MyService { get; private set; } = null!

    public ValueTask InitializeAsync() => ValueTask defaults;
    public ValueTask DisposeAsync() => ValueTask defaults;
}