using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// 1. Define the interface
public interface IDataLoader
{
    Task<string> LoadAsync(string source, CancellationToken ct);
}

// 2. Define the class
public class ParallelDataService
{
    private readonly IDataLoader loaderA;
    private readonly IDataLoader loaderB;
    private readonly IDataLoader loaderC;

    // Constructor accepting exactly three IDataLoader parameters
    public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
    {
        this.loaderA = loaderA;
        this.loaderB = loaderB;
        this.loaderC = loaderC;
    }

    // Method to load all in parallel
    public async Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        // Create a list to store results in the correct order
        List<string> results = new List<string>();

        // Create tasks for all loaders
        var taskA = loaderA.LoadAsync("sourceA", ct);
        var taskB = loaderB.LoadAsync("sourceB", ct);
        var taskC = loaderC.LoadAsync("sourceC", ct);

        // Wait for all tasks to complete
        await Task.WhenAll(taskA, taskB, taskC);

        // Return results in the order [resultA, resultB, resultC]
        results.Add(await taskA);
        results.Add(await taskB);
        results.Add(await taskC);

        return results;
    }
}