using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class AsyncResourcePool : IAsyncLifetime
{
    public List<string> AvailableResources { get; } = new();

    public ValueTask InitializeAsync()
    {
        AvailableResources.Add("resource-1");
        AvailableResources.Add("resource-2");
        AvailableResources.Add("resource-3");
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        AvailableResources.Clear();
        return ValueTask.CompletedTask;
    }

    public string? Acquire()
    {
        if (AvailableResources.Count == 0)
        {
            return null;
        }

        int lastIndex = AvailableResources.Count - 1;
        string resource = AvailableResources[lastIndex];
        AvailableResources.RemoveAt(lastIndex);
        return resource;
    }
}