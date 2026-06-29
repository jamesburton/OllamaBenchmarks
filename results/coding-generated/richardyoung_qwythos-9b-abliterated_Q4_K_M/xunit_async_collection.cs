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
public class CounterCollection : ICollectionFixture<CounterFixture> {
    // No test methods, just implement interface members
    public ValueTask InitializeAsync() => default;
    public ValueTask DisposeAsync() => default;
}

[Collection("Counter")]
public class CounterFirstTests(CounterFixture fixture) {
    [Fact]
    public void Test() {
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterOrEqualTo(1);
    }
}

[Collection("Counter")]
public class CounterSecondTests(CounterFixture fixture) {
    [Fact]
    public void Test() {
        fixture.Counter.Increment();
        fixture.Counter.Increment();
        fixture.Counter.Count.Should().BeGreaterOrEqualTo(2);
    }
}