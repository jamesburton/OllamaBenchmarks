using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

public class AsyncSetup : IAsyncLifetime
{
    public List<string> InitializedItems { get; } = new();

    public ValueTask InitializeAsync()
    {
        InitializedItems.Add("item1");
        InitializedItems.Add("item2");
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        InitializedItems.Clear();
        return ValueTask.CompletedTask;
    }
}