public class InMemoryStore : IAsyncLifetime
        {
            public Dictionary<string, string> Data { get; private set; } = new();
            public ValueTask InitializeAsync() => new ValueTask(InitializeAsyncImpl()); // Wait, simpler: just return default if async logic is trivial or use a helper.
            // Actually, the reference shows: public ValueTask InitializeAsync() { ... return default; }
            // I need to seed data. Since it's synchronous seeding in the method body but returning ValueTask, I can just do it synchronously and return default(ValueTask).
            // However, usually async methods imply async work. But the reference example returns `default` explicitly.
            // Let's follow the reference style: public ValueTask InitializeAsync() { ... }
        }

public ValueTask InitializeAsync() => new ValueTask(InitializeAsyncImpl()); // No, that's not how it works usually.
        // Actually, the reference says: public ValueTask InitializeAsync() { ... return default; }
        // It implies the method body executes synchronously but returns a ValueTask.
        // To seed data, I can do it in the body.

[ClassFixture(typeof(InMemoryStore))]
            public class StoreServiceTests {
                private readonly InMemoryStore _fixture;
                public StoreServiceTests(InMemoryStore fixture) { _fixture = fixture; }
            }

[ClassFixture(typeof(InMemoryStore))]
            public class StoreServiceTests {
                private readonly InMemoryStore _fixture;
                public StoreServiceTests(InMemoryStore fixture) { _fixture = fixture; }
            }

public class InMemoryStore : System.Runtime.CompilerServices.IAsyncLifetime // Wait, can't use using.