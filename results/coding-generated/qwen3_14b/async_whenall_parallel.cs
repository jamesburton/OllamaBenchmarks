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

    public async Task<List<string>> LoadAllAsync(CancellationToken ct)
    {
        var taskA = loaderA.LoadAsync("sourceA", ct);
        var taskB = loaderB.LoadAsync("sourceB", ct);
        var taskC = loaderC.LoadAsync("sourceC", ct);

        var results = await Task.WhenAll(taskA, taskB, taskC);
        return new List<string>(results);
    }
}