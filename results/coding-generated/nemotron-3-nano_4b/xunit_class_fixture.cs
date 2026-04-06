public class InMemoryStore : IAsyncLifetime  
{  
    public Dictionary<string, string> Data { get; private set; } = new();  

    public ValueTask InitializeAsync() => default;  
    public ValueTask DisposeAsync() => default;  

    public void Initialize()  
    {  
        Data = new() { { "" key1 "" \"value1\" }, { "" key2 "" \"value2\" } };  
    }  
}  

public class StoreService  
{  
    private readonly InMemoryStore store;  
    public StoreService(InMemoryStore store) => this.store = store;  

    public string? Get(string key) => store.Data.TryGetValue(key, out var v) ? v : null;  
    public void Set(string key, string value) => store.Data[key] = value;  
}  

public class StoreServiceTests  
{  
    private readonly StoreService service;  
    private readonly InMemoryStore store;  
    public StoreServiceTests(InMemoryStore store)  
    {  
        this.store = store;  
        service = new StoreService(store);  
    }  

    [Fact]  
    void Get_ExistingKey_ReturnsValue()  
    {  
        store.Service.Service.Get("key1") .Should().Be("value1");  
    }  

    [Fact]  
    void Set_ThenGet_ReturnsSetValue()  
    {  
        store.Service.Service.Set("key3", "value3");  
        store.Service.Service.Get("key3") .Should().Be("value3");  
    }  

    [Fact]  
    void Get_NonexistentKey_ReturnsNull()  
    {  
        store.Service.Service.Get("nonexistent") .Should().BeNull();  
    }  
}