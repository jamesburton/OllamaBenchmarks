using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

interface IDataLoader
{
    Task<string> LoadAsync(string source, CancellationToken ct);
}

class ParallelDataService : IDataLoader
{
    private readonly IDataLoader loaderA;
    private readonly IDataLoader loaderB;
    private readonly IDataLoader loaderC;

    public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
    {
        this.loaderA = loaderA ?? throw new ArgumentNullException(nameof(loaderA));
        this.loaderB = loaderB ?? throw new ArgumentNullException(nameof(loaderB));
        this.loaderC = loaderC ?? throw new ArgumentNullException(nameof(loaderC));
    }

    public Task<string> LoadAsync(string source, CancellationToken ct)
    {
        return null;
    }

    public async Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        var tasks = new List<Task<string>>
            {loaderA.LoadAsync("sourceA", ct),
             loaderB.LoadAsync("sourceB", ct),
             loaderC.LoadAsync("sourceC", ct)};

        await Task.WhenAll(tasks);

        return new List<string>(tasks.Select(task => task.Result));
    }
}