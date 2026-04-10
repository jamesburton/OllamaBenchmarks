[assembly: AssemblyFixture(typeof(DatabaseFixture))]
        public class MyTests(DatabaseFixture db) { }

[assembly: AssemblyFixture(typeof(DatabaseFixture))]
        public class MyTests(DatabaseFixture db) { }

public class MyCollection : ICollectionFixture<MyFixture> { }
        [CollectionDefinition("MyCollection")]
        public class MyCollection { }

[CollectionDefinition("Counter")]
        public class CounterCollection : ICollectionFixture<CounterFixture> { }

public ValueTask InitializeAsync() { ... return default; }
        public ValueTask DisposeAsync() { ... return default; }

[CollectionDefinition("Counter")]
    public class CounterCollection : ICollectionFixture<CounterFixture> { }

public interface ICollectionFixture<T> { }

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

public class CounterFixture : IAsyncLifetime
{
    public CounterService Counter { get; private set; } = null!;

    public ValueTask InitializeAsync()
    {
        Counter = new CounterService();
        return default;
    }

    public ValueTask DisposeAsync()
    {
        return default;
    }
}

[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture>
{
}

[Collection("Counter")]
public class CounterFirstTests(CounterFixture fixture)
{
    [Fact]
    public void Test_Should_Increment()
    {
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterOrEqualTo(1);
    }
}

[Collection("Counter")]
public class CounterSecondTests(CounterFixture fixture)
{
    [Fact]
    public void Test_Should_IncrementTwice()
    {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterOrEqualTo(2);
    }
}

result.Should().Be(42);

result.Should().Be(42);
obj.Should().BeNull();