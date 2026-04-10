using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// Mock interfaces/classes needed for compilation context, assuming they exist elsewhere
// IAsyncLifetime is defined by xUnit v3 context
// IClassFixture is defined by xUnit v3 context
// IEntity, IAsyncLifetime, etc., are assumed to be defined elsewhere if needed by AwesomeAssertions

// 1. InMemoryStore implementing IAsyncLifetime
public class InMemoryStore : IAsyncLifetime
{
    public Dictionary<string, string> Data { get; private set; } = new();

    public ValueTask InitializeAsync()
    {
        Data.Add("key1", "value1");
        Data.Add("key2", "value2");
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Data.Clear();
        return ValueTask.CompletedTask;
    }
}

// 2. StoreService class
public class StoreService
{
    private readonly InMemoryStore _store;

    public StoreService(InMemoryStore store)
    {
        _store = store;
    }

    public string? Get(string key)
    {
        if (_store.Data.TryGetValue(key, out var value))
        {
            return value;
        }
        return null;
    }

    public void Set(string key, string value)
    {
        _store.Data[key] = value;
    }
}

// 3. Test class StoreServiceTests implementing IClassFixture<InMemoryStore>
public class StoreServiceTests : IClassFixture<InMemoryStore>
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
        string result = _service.Get("key1");
        result.Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        const string key = "key3";
        const string value = "value3";

        _service.Set(key, value);
        string result = _service.Get(key);
        result.Should().Be(value);
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        string result = _service.Get("nonexistent");
        result.Should().BeNull();
    }
}