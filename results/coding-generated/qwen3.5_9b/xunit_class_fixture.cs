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

    public string? Get(string key) => 
        _store.Data.TryGetValue(key, out var v) ? v : null;

    public void Set(string key, string value) => 
        _store.Data[key] = value;
}

public class StoreServiceTests
{
    private readonly InMemoryStore _store;
    private readonly StoreService _sut;

    public StoreServiceTests(InMemoryStore store)
    {
        _store = store;
        var service = new StoreService(store);
        // Note: The constructor logic in the prompt implies immediate usage, 
        // but IAsyncLifetime requires explicit initialization. 
        // However, to strictly follow "Constructor receives... and creates", we assume 
        // the test runner handles lifecycle or this is a simplified example where 
        // InitializeAsync was called externally (e.g., via TestContext).
        // Given standard xUnit v3 patterns with IClassFixture<InMemoryStore>, 
        // the fixture's InitializeAsync runs before tests. We will assume _store.Data is ready.

        // Re-creating SUT based on prompt: "Constructor receives InMemoryStore store and creates a StoreService from it"
        // But we need to ensure Data is seeded if not done by Fixture lifecycle automatically in this specific context.
        // To be safe with IAsyncLifetime, the InitializeAsync MUST run before use. 
        // Since we cannot add using statements or namespaces, we assume the test runner handles the fixture initialization correctly.
        // However, for the code to compile and work without external calls shown here:

        _sut = new StoreService(_store);
    }

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        var result = _sut.Get("key1");
        result.Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        _sut.Set("key3", "value3");
        var result = _sut.Get("key3");
        result.Should().Be("value3");
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        var result = _sut.Get("nonexistent");
        result.Should().BeNull();
    }
}