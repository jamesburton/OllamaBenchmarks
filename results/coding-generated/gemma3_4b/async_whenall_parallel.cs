public interface IDataLoader
{
    Task<string> LoadAsync(string source, CancellationToken ct);
}

public class ParallelDataService
{
    private readonly IDataLoader loaderA;
    private readonly IDataLoader loaderB;
    private readonly IDataLoader loaderC;

    public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
    {
        this.loaderA = loaderA;
        this.loaderB = loaderB;
        this.loaderC = loaderC;
    }

    public Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        return Task.WhenAll(
            loaderA.LoadAsync("sourceA", ct),
            loaderB.LoadAsync("sourceB", ct),
            loaderC.LoadAsync("sourceC", ct)
        ).ContinueWith(result =>
        {
            List<string> results = new List<string>();
            results.Add(result.Result1);
            results.Add(result.Result2);
            results.Add(result.Result3);
            return results;
        });
    }
}