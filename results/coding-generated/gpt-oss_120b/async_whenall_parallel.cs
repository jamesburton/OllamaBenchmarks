public interface IDataLoader
{
    System.Threading.Tasks.Task<string> LoadAsync(string source, System.Threading.CancellationToken ct);
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

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
    {
        var results = await System.Threading.Tasks.Task.WhenAll(
            _loaderA.LoadAsync("sourceA", ct),
            _loaderB.LoadAsync("sourceB", ct),
            _loaderC.LoadAsync("sourceC", ct));

        return new System.Collections.Generic.List<string>(results);
    }
}