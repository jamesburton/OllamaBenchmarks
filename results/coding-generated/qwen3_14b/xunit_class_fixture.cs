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
        return store.Data.TryGetValue(key, out var value) ? value : null;
    }

    public void Set(string key, string value)
    {
        store.Data[key] = value;
    }
}

public class StoreServiceTests : IClassFixture<InMemoryStore>
{
    private readonly StoreService service;

    public StoreServiceTests(InMemoryStore store)
    {
        service = new StoreService(store);
    }

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        service.Get("key1").Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        service.Set("key3", "value3");
        service.Get("key3").Should().Be("value3");
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        service.Get("nonexistent").Should().BeNull();
    }
}