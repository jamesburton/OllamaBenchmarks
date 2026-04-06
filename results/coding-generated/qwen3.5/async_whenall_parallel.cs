public interface IDataLoader
    {
        Task<string> LoadAsync(string source, CancellationToken ct);
    }

public ParallelDataService(IDataLoader loaderA, IDataLoader loaderB, IDataLoader loaderC)
        {
            this.loaderA = loaderA;
            this.loaderB = loaderB;
            this.loaderC = loaderC;
        }

var tasks = new[]
            {
                loaderA.LoadAsync("sourceA", ct),
                loaderB.LoadAsync("sourceB", ct),
                loaderC.LoadAsync("sourceC", ct)
            };
            var results = await Task.WhenAll(tasks);
            return results.ToList(); // or just cast to List

public class ParallelDataService
        {
            private System.Threading.Tasks.Task<string> LoadAsync(string source, System.Threading.CancellationToken ct)
            {
                // Wait, this is the interface method signature.
            }
        }