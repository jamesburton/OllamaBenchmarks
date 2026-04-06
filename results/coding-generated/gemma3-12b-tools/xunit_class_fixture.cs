public interface IAsyncLifetime
{
    ValueTask InitializeAsync();
    ValueTask DisposeAsync();
}

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
        return _store.Data.TryGetValue(key, out var v) ? v : null;
    }

    public void Set(string key, string value)
    {
        _store.Data[key] = value;
    }
}

public class StoreServiceTests : IClassFixture<InMemoryStore>
{
    private readonly StoreService _service;

    public StoreServiceTests(InMemoryStore store)
    {
        _service = new StoreService(store);
    }

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        // Arrange
        // Act
        string? result = _service.Get("key1");
        // Assert
        result.Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        // Arrange
        // Act
        _service.Set("key3", "value3");
        string? result = _service.Get("key3");
        // Assert
        result.Should().Be("value3");
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        // Arrange
        // Act
        string? result = _service.Get("nonexistent");
        // Assert
        result.Should().BeNull();
    }
}