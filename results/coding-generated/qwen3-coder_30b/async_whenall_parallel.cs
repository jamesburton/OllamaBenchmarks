using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

interface IDataLoader
{
    Task<string> LoadAsync(string source, CancellationToken ct);
}

class ParallelDataService
{
    private readonly IDataLoader _loaderA;
    private readonly IDataLoader _loaderB;
    private readonly IDataLoader _loaderC;

    public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
    {
        _loaderA = loaderA;
        _loaderB = loaderB;
        _loaderC = loaderC;
    }

    public async Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        var taskA = _loaderA.LoadAsync("sourceA", ct);
        var taskB = _loaderB.LoadAsync("sourceB", ct);
        var taskC = _loaderC.LoadAsync("sourceC", ct);

        var results = await Task.WhenAll(taskA, taskB, taskC);

        return new List<string> { results[0], results[1], results[2] };
    }
}