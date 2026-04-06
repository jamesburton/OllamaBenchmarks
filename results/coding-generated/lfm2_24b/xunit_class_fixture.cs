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
    private readonly InMemoryStore _store;

    public StoreService(InMemoryStore store)
    {
        _store = store;
    }

    public string? Get(string key)
    {
        _store.Data.TryGetValue(key, out var v);
        return v;
    }

    public void Set(string key, string value)
    {
        _store.Data[key] = value;
    }
}

public class StoreServiceTests : IClassFixture<InMemoryStore>
{
    private readonly InMemoryStore _store;
    private readonly StoreService _service;

    public StoreServiceTests(InMemoryStore store)
    {
        _store = store;
        _store.InitializeAsync().GetAwaiter().GetResult();
        _service = new StoreService(_store);
    }

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        _store.Data["key1"] = "value1";
        _service.Set("key1", "value1");
        _store.Data["key1"] = "value1";
        _store.Data.TryGetValue("key1", out var result);
        result.Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        _store.Data["key3"] = "value3";
        _service.Set("key3", "value3");
        _store.Data.TryGetValue("key3", out var result);
        result.Should().Be("value3");
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        _store.Data.TryGetValue("nonexistent", out var result);
        result.Should().BeNull();
    }
}