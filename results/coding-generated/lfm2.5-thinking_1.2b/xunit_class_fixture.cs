public class InMemoryStore : IAsyncLifetime
{
    private Dictionary<string, string> data = new Dictionary<string, string>();
    public ValueTask InitializeAsync() => new ValueTask(data);
    public ValueTask DisposeAsync() => ValueTask.Empty;
}

public class StoreService : IClassFixture<InMemoryStore>
{
    private readonly InMemoryStore _store;
    public StoreService(InMemoryStore store) : _store(store) { }
    public string? Get(string key) => _store.Data.TryGetValue(key, out var v) ? v : null;
    public void Set(string key, string value) => _store.Data[key] = value;
}

[TestClass]
public class TestServiceTests : IClassFixture<InMemoryStore>
{
    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        var store = new InMemoryStore();
        Assert.Equal("value1", store.Get("key1"));
    }
    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        var store = new InMemoryStore();
        store.Set("key3", "value3");
        Assert.Equal("value3", store.Get("key3"));
    }
    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        var store = new InMemoryStore();
        Assert.Empty(store.Get("nonexistent"));
    }
}