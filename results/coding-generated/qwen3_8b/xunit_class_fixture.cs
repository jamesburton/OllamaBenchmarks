public class InMemoryStore : IAsyncLifetime
{
    public Dictionary<string, string> Data { get; private set; } = new();

    public ValueTask InitializeAsync()
    {
        Data = new Dictionary<string, string>
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };
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
    private readonly InMemoryStore _store;

    public StoreService(InMemoryStore store)
    {
        _store = store;
    }

    public string? Get(string key)
    {
        if (_store.Data.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }

    public void Set(string key, string value)
    {
        _store.Data[key] = value;
    }
}

public class StoreServiceTests
{
    private readonly StoreService _service;

    public StoreServiceTests(InMemoryStore store)
    {
        _service = new StoreService(store);
    }

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        var result = _service.Get("key1");
        result.Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        _service.Set("key3", "value3");
        var result = _service.Get("key3");
        result.Should().Be("value3");
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        var result = _service.Get("nonexistent");
        result.Should().BeNull();
    }
}