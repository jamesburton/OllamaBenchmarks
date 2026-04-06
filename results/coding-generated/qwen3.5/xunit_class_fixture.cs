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
        public StoreService(InMemoryStore store)
        {
            Store = store;
        }

        public string? Get(string key)
        {
            return Store.Data.TryGetValue(key, out var v) ? v : null;
        }

        public void Set(string key, string value)
        {
            Store.Data[key] = value;
        }

        private InMemoryStore Store { get; }
    }

    public class StoreServiceTests : IClassFixture<InMemoryStore>
    {
        private readonly InMemoryStore _store;
        private readonly StoreService _sut;

        public StoreServiceTests(InMemoryStore store)
        {
            _store = store;
            _sut = new StoreService(store);
        }

        [Fact]
        public void Get_ExistingKey_ReturnsValue()
        {
            _sut.Get("key1").Should().Be("value1");
        }

        [Fact]
        public void Set_ThenGet_ReturnsSetValue()
        {
            _sut.Set("key3", "value3");
            _sut.Get("key3").Should().Be("value3");
        }

        [Fact]
        public void Get_NonexistentKey_ReturnsNull()
        {
            _sut.Get("nonexistent").Should().BeNull();
        }
    }