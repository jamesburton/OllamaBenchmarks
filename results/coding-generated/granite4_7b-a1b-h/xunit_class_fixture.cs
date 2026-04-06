using Xunit;
using AwesomeAssertions;

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

    public string? Get(string key) => store.Data.TryGetValue(key, out var v) ? v : null;

    public void Set(string key, string value) => store.Data[key] = value;
}

public class StoreServiceTests
{
    private readonly InMemoryStore store;
    private readonly StoreService service;

    public StoreServiceTests(InMemoryStore store)
    {
        this.store = store;
        service = new StoreService(store);
    }

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        var value = service.Get("key1");
        value.Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        service.Set("key3", "value3");
        var value = service.Get("key3");
        value.Should().Be("value3");
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        var value = service.Get("nonexistent");
        Assert.Null(value);
    }
}