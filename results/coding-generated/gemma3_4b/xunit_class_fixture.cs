using AwesomeAssertions;
using Xunit;

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
    public StoreService(InMemoryStore store)
    {
    }

    public string? Get(string key)
    {
        return store.Data.TryGetValue(key, out var v) ? v : null;
    }

    public void Set(string key, string value)
    {
        store.Data[key] = value;
    }
}

public class StoreServiceTests
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
        string value = _service.Get("key1");
        value.Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        _service.Set("key3", "value3");
        string value = _service.Get("key3");
        value.Should().Be("value3");
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        string value = _service.Get("nonexistent");
        value.Should().BeNull();
    }
}