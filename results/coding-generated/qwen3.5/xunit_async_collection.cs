public class CounterService
        {
            public int Count { get; private set; } = 0;
            public void Increment() => Count++;
            public void Reset() => Count = 0;
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
            public CounterFixture GetFixture() => new CounterFixture();
        }

[assembly: AssemblyFixture(typeof(DatabaseFixture))]
        public class MyTests(DatabaseFixture db) { }

[CollectionDefinition("Counter")]
        public class CounterCollection : ICollectionFixture<CounterFixture>
        {
            // Implementation needed to provide the instance
        }

[CollectionDefinition("Counter")]
        public class CounterCollection : ICollectionFixture<CounterFixture>
        {
            public CounterFixture GetFixture() => new CounterFixture();
        }

[CollectionDefinition("MyCollection")]
        public class MyCollection : ICollectionFixture<MyFixture>
        {
            public MyFixture GetFixture() => new MyFixture();
        }

[CollectionDefinition("Counter")]
    public class CounterCollection : ICollectionFixture<CounterFixture>
    {
        public CounterFixture GetFixture() => new CounterFixture();
    }