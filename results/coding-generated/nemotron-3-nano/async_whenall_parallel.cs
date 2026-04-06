public interface IDataLoader
{
    System.Threading.Tasks.Task<string> LoadAsync(string source, System.Threading.CancellationToken ct);
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

    public async System.Threading.Tasks.Task<List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
    {
        var taskA = loaderA.LoadAsync("sourceA", ct);
        var taskB = loaderB.LoadAsync("sourceB", ct);
        var taskC = loaderC.LoadAsync("sourceC", ct);

        var results = await System.Threading.Tasks.Task.WhenAll(taskA, taskB, taskC);
        return new System.Collections.Generic.List<string> { results[0], results[1], results[2] };
    }
}