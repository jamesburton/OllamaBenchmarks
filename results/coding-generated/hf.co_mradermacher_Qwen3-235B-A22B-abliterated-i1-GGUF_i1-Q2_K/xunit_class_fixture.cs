public class InMemoryStore : IAsyncLifetime
{
    public Dictionary<string, string> Data { get; private set; } = new();

    public ValueTask InitializeAsync()
    {
        Data.Add("key1", "value1");
        Data.Add("key2", "value2");
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
        return _store.Data.TryGetValue(key, out var value) ? value : null;
    }

    public void Set(string key, string value)
    {
        _store.Data[key] = value;
    }
}

public class StoreServiceTests : IClassFixture<InMemoryStore>
{
    private readonly StoreService _storeService;

    public StoreServiceTests(InMemoryStore store)
    {
        _storeService = new StoreService(store);
    }

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        var result = _storeService.Get("key1");
        result.Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        _storeService.Set("key3", "value3");
        var result = _storeService.Get("key3");
        result.Should().Be("value3");
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        var result = _storeService.Get("nonexistent");
        result.Should().BeNull();
    }
}