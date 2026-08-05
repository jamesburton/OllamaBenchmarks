using Xunit
using AwesomeAssertions

public class InMemoryStore : IAsyncLifetime
{
    public Dictionary<string, string> Data { get; private set; } = new()
    {
        { "key1", "value1" },
        { "key2", "value2" }
    }

    public ValueTask InitializeAsync() => default
    public ValueTask DisposeAsync() => default
}

public class StoreService
{
    public StoreService(InMemoryStore store)
    {
    }

    public string? Get(string key)
    {
        store.Data.TryGetValue(key, out var v) ? v : null
    }

    public void Set(string key, string value)
    {
        store.Data[key] = value
    }
}

public class StoreServiceTests : IClassFixture<InMemoryStore>
{
    private InMemoryStore store
    private StoreService service

    public StoreServiceTests(InMemoryStore store)
    {
        this.store = store
        this.service = new StoreService(store)
    }

    [Fact] Get_ExistingKey_ReturnsValue
    {
        var result = service.Get("key1")
        AwesomeAssertions.Assert(result == "value1"
    }

    [Fact] Set_ThenGet_ReturnsSetValue
    {
        service.Set("key3", "value3")
        var result = service.Get("key3")
        AwesomeAssertions.Assert(result == "value3"
    }

    [Fact] Get_NonexistentKey_ReturnsNull
    {
        var result = service.Get("nonexistent")
        AwesomeAssertions.Assert(result == null
    }
}