public class InMemoryStore : IAsyncLifetime
            {
                public Dictionary<string, string> Data { get; private set; } = new();
                public ValueTask InitializeAsync()
                {
                    Data["key1"] = "value1";
                    Data["key2"] = "value2";
                    return default;
                }
                public ValueTask DisposeAsync()
                {
                    Data.Clear();
                    return default;
                }
            }

public class StoreService
            {
                private readonly InMemoryStore store;
                public StoreService(InMemoryStore store)
                {
                    this.store = store;
                }
                public string? Get(string key)
                {
                    return store.Data.TryGetValue(key, out var v) ? v : null;
                }
                public void Set(string key, string value)
                {
                    store.Data[key] = value;
                }
            }

public class StoreServiceTests : IClassFixture<InMemoryStore>
            {
                private readonly StoreService sut;
                private readonly InMemoryStore store;

                public StoreServiceTests(InMemoryStore store)
                {
                    this.store = store;
                    this.sut = new StoreService(store);
                }

                [Fact]
                public void Get_ExistingKey_ReturnsValue()
                {
                    sut.Get("key1").Should().Be("value1");
                }

                [Fact]
                public void Set_ThenGet_ReturnsSetValue()
                {
                    sut.Set("key3", "value3");
                    sut.Get("key3").Should().Be("value3");
                }

                [Fact]
                public void Get_NonexistentKey_ReturnsNull()
                {
                    sut.Get("nonexistent").Should().BeNull();
                }
            }

public StoreServiceTests(InMemoryStore store)
    {
        this.store = store;
        this.sut = new StoreService(store);
    }