public class InMemoryStore : IAsyncLifetime
{
    public Dictionary<string, string> Data { get; private set; } = new()
    {
        {"key1", "value1"},
        {"key2", "value2"}
    };

    public ValueTask InitializeAsync() => Task.CompletedTask;

    public ValueTask DisposeAsync() => Task.CompletedTask;
}