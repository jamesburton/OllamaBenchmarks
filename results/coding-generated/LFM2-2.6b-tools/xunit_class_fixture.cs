public class InMemoryStore : IAsyncLifetime
{
    public Dictionary<string, string> Data { get; private set; } = new();

    public ValueTask InitializeAsync() => ValueTask.Default;
    public ValueTask DisposeAsync() => ValueTask.Default;
}

public class StoreService
{
    private readonly InMemoryStore _store;

    public StoreService(InMemoryStore store) => _store = store;

    public string? Get(string key) => _store.Data.TryGetValue(key, out var v) ? v : null;

    public void Set(string key, string value) => _store.Data[key] = value;
}

public class StoreServiceTests
{
    private readonly InMemoryStore _store;
    private readonly StoreService _service;

    public StoreServiceTests() => _store = new(), _service = new(_store);

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        _store.Set("key1", "value1");
        _service.Set("key1", "value1");
        _store.Set("key1", "value1");
        _service.Set("key1", "value1");
        _store.Set("key2", "value2");

        _service.Set("key3", "value3");
        _store.Set("key3", "value3");

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _service.Set("key1", "value1");
        _store.Set("key1", "value1");

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key1"] = "value1";
        _store.Data["key2"] = "value2";
        _store.Data["key3"] = "value3";

        _store.Data["key