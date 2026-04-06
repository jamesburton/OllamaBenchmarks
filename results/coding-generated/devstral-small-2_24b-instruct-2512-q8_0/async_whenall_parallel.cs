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
        var resultA = _loaderA.LoadAsync("sourceA", ct);
        var resultB = _loaderB.LoadAsync("sourceB", ct);
        var resultC = _loaderC.LoadAsync("sourceC", ct);

        await Task.WhenAll(resultA, resultB, resultC);

        return new List<string> { resultA.Result, resultB.Result, resultC.Result };
    }
}