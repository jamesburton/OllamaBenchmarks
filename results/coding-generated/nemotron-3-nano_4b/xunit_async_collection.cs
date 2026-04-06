public class CounterService {
    public int Count { get; private set; } = 0;
    public void Increment() => Count++;
    public void Reset() => Count = 0;
}

public class CounterFixture : IAsyncLifetime {
    public CounterService Counter { get; private set; } = null!;
    public ValueTask InitializeAsync() => default;
    public ValueTask DisposeAsync() => default;
}

[CollectionDefinition("Counter")]
public class CounterCollection : ICollectionFixture<CounterFixture> { }

public class CounterFirstTests {
    private readonly CounterFixture Fixture;
    public CounterFirstTests(CounterFixture fixture) => Fixture = fixture;

    [Collection("Counter")]
    [Fact]
    public void Increment_RaisesCount_OneOrMore() {
        Fixture.Counter.Increment();
        Fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(1);
    }
}

public class CounterSecondTests {
    private readonly CounterFixture Fixture;
    public CounterSecondTests(CounterFixture fixture) => Fixture = fixture;

    [Collection("Counter")]
    [Fact]
    public void Increment_Twice_RaisesCount_TwoOrMore() {
        Fixture.Counter.Increment();
        Fixture.Counter.Increment();
        Fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
    }
}