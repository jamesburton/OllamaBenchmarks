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
        _loaderA = loaderA;
        _loaderB = loaderB;
        _loaderC = loaderC;
    }

    public async Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        var tasks = new Task<string>[]
        {
            _loaderA.LoadAsync("sourceA", ct),
            _loaderB.LoadAsync("sourceB", ct),
            _loaderC.LoadAsync("sourceC", ct)
        };

        await Task.WhenAll(tasks);

        return new List<string> { tasks[0].Result, tasks[1].Result, tasks[2].Result };
    }
}