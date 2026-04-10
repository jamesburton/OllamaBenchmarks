public class CounterService {
            public int Count { get; private set; } = 0;
            public void Increment() => Count++;
            public void Reset() => Count = 0;
        }

public class CounterFixture : IAsyncLifetime {
            public CounterService Counter { get; private set; } = null!;
            public ValueTask InitializeAsync() {
                Counter = new CounterService();
                return default;
            }
            public ValueTask DisposeAsync() {
                return default;
            }
        }

[CollectionDefinition("Counter")]
        public class CounterCollection : ICollectionFixture<CounterFixture> { }