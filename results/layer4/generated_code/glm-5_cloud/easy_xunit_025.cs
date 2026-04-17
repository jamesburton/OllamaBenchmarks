using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class AsyncDatabase : IAsyncLifetime
{
    public List<string> Records { get; } = new();

    public ValueTask InitializeAsync()
    {
        Records.Add("seed1");
        Records.Add("seed2");
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Records.Clear();
        return ValueTask.CompletedTask;
    }

    public void Insert(string record)
    {
        Records.Add(record);
    }
}