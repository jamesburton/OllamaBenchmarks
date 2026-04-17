using System.Threading.Tasks;
using Xunit;

public class AsyncTestFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = "";
    public bool IsConnected { get; private set; }

    public ValueTask InitializeAsync()
    {
        ConnectionString = "Server=test;Database=db";
        IsConnected = true;
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        IsConnected = false;
        ConnectionString = "";
        return ValueTask.CompletedTask;
    }
}