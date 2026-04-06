public class InMemoryStore : IAsyncLifetime
{
    public Dictionary<string, string> Data { get; private set; } = new();

    public ValueTask InitializeAsync()
    {
        Data["key1"] = "value1";
        Data["key2"] = "value2";
        return default;
    }

    public ValueTask DisposeAsync() => Data.Clear().AsValueTask();
}

public class StoreService
{
    private readonly InMemoryStore _store;

    public StoreService(InMemoryStore store) => _store = store;

    public string? Get(string key)
    {
        if (_store.Data.TryGetValue(key, out var value))
            return value;

        return null;
    }

    public void Set(string key, string value) => _store.Data[key] = value;
}

public class StoreServiceTests : IClassFixture<InMemoryStore>
{
    private readonly InMemoryStore _store;
    private readonly StoreService _service;

    public StoreServiceTests(InMemoryStore store)
    {
        _store = store;
        _service = new StoreService(store);
    }

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        _service.Get("key1").Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        _service.Set("key3", "value3");
        _service.Get("key3").Should().Be("value3");
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        _service.Get("nonexistent").Should().BeNull();
    }
}
</generate>