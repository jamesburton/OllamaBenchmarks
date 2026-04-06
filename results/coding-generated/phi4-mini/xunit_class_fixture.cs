using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InMemoryStore : IAsyncLifetime
{
    public Dictionary<string, string> Data { get; private set; } = new();

    public ValueTask InitializeAsync() => Task.CompletedTask;

    public ValueTask DisposeAsync() => Task.CompletedTask;
}

public class StoreService
{
    private readonly InMemoryStore store;

    public StoreService(InMemoryStore store)
    {
        this.store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public string? Get(string key) =>
        store.Data.TryGetValue(key, out var value) ? (string?)value : null;

    public void Set(string key, string value)
    {
        if (store is not { Data: _ })
            throw new InvalidOperationException("Store has been disposed.");

        store.Data[key] = value;
    }
}

public class StoreServiceTests
{
    private readonly InMemoryStore storeFixture;
    private readonly StoreService service;

    public StoreServiceTests(InMemoryStore store)
    {
        this.storeFixture = store ?? throw new ArgumentNullException(nameof(store));
        this.service = new(StoreService(this.storeFixture));
    }

    [Fact]
    public void Get_ExistingKey_ReturnsValue()
    {
        var result = service.Get("key1");
        result.Should().NotBeNull();
        result.Should().Be("value1");
    }

    [Fact]
    public void Set_ThenGet_ReturnsSetValue()
    {
        this.service.Set("key3", "value3");
        var result = this.service.Get("key3");
        result.Should().Be("value3");
    }

    [Fact]
    public void Get_NonexistentKey_ReturnsNull()
    {
        var result = this.service.Get("nonexistent");
        result.Should().BeNull();
    }
}