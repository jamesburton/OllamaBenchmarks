using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InMemoryStore
{
    public class InMemoryStore : IAsyncLifetime
    {
        private readonly Dictionary<string, string> _data = new();

        public ValueTask InitializeAsync()
        {
            _data.Add("key1", "value1");
            _data.Add("key2", "value2");
            return default;
        }

        public ValueTask DisposeAsync()
        {
            _data.Clear();
            return default;
        }
    }

    using (var store = new InMemoryStore())
    {
        var service = new StoreService(store);

        [Fact]
        public void Get_ExistingKey_ReturnsValue()
        {
            Assert.Equal("value1", service.Get("key1"));
        }

        [Fact]
        public void Set_ThenGet_ReturnsSetValue()
        {
            store.Set("key3", "value3");
            Assert.Equal("value3", service.Get("key3"));
        }

        [Fact]
        public void Get_NonexistentKey_ReturnsNull()
        {
            Assert.Null(service.Get("nonexistent"));
        }
    }
}

public class StoreService
{
    private readonly InMemoryStore _store;

    public StoreService(InMemoryStore store)
    {
        _store = store;
    }

    public string? Get(string key) => _store.Data.TryGetValue(key, out var value) ? value : null;

    public void Set(string key, string value) => _store.Data[key] = value;
}