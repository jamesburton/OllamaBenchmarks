public class InMemoryStore : IAsyncLifetime {
            public Dictionary<string, string> Data { get; private set; } = new();
            public async ValueTask InitializeAsync() {
                Data.Add("key1", "value1");
                Data.Add("key2", "value2");
                return default;
            }
            public async ValueTask DisposeAsync() {
                Data.Clear();
                return default;
            }
        }

public class StoreService {
            private readonly InMemoryStore _store;
            public StoreService(InMemoryStore store) => _P_store = store;
            public string? Get(string key) => _store.Data.TryGetValue(key, out var v) ? v : null;
            public void Set(string key, string value) => _store.Data[key] = value;
        }