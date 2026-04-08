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

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<string>> LoadAllAsync(System.Threading.CancellationToken ct)
    {
        var taskA = loaderA.LoadAsync("sourceA", ct);
        var taskB = loaderB.LoadAsync("sourceB", ct);
        var taskC = loaderC.LoadAsync("sourceC", ct);

        System.Threading.Tasks.Task<string>[] tasks = new System.Threading.Tasks.Task<string>[] { taskA, taskB, taskC };
        string[] resultsArray = await System.Threading.Tasks.Task.WhenAll(tasks);

        return new System.Collections.Generic.List<string>(resultsArray);
    }
}