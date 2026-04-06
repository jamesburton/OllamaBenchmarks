using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IDataLoader
{
    Task<string> LoadAsync(string source, CancellationToken ct);
}

public class ParallelDataService
{
    private readonly IDataLoader _loaderA;
    private readonly IDataLoader _loaderB;
    private readonly IDataLoader _loaderC;

    public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
    {
        _loaderA = loaderA ?? throw new ArgumentNullException(nameof(loaderA));
        _loaderB = loaderB ?? throw new ArgumentNullException(nameof(loaderB));
        _loaderC = loaderC ?? throw new ArgumentNullException(nameof(loaderC));
    }

    public async Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        var taskA = _loaderA.LoadAsync("sourceA", ct);
        var taskB = _loaderB.LoadAsync("sourceB", ct);
        var taskC = _loaderC.LoadAsync("sourceC", ct);

        await Task.WhenAll(taskA, taskB, taskC);

        return new List<string>
        {
            await taskA,
            await taskB,
            await taskC
        };
    }
}