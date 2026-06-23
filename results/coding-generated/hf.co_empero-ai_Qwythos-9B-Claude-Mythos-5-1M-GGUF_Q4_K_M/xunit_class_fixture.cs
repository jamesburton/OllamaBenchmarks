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

    public StoreService(InMemoryStore store) => _store = store;

    public string? Get(string key)
        => _store.Data.TryGetValue(key, out var v) ? v : null;

    public void Set(string key, string value)
        => _store.Data[key] = value;
}

public class StoreServiceTests : IClassFixture<InMemoryStore>
{
    private readonly InMemoryStore _store;
    private readonly StoreService sut;

    public StoreServiceTests(InMemoryStore store)
    {
        _store = store;
        sut = new StoreService(store);
    }

    [Fact] Get_ExistingKey_ReturnsValue() => sut.Get("key1").Should().Be("value1");

    [Fact] Set_ThenGet_ReturnsSetValue() => {
        sut.Set("key3", "value3");
        sut.Get("key3").Should().Be("value3");
    }

    [Fact] Get_NonexistentKey_ReturnsNull() => sut.Get("nonexistent").Should().BeNull();
}