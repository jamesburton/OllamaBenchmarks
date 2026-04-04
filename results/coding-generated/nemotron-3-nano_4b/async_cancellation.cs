using System.Runtime.CompilerServices;

public interface IDataSource
{
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
}

public class DataProcessor
{
    public async IAsyncEnumerable<string> ProcessAllBatchesAsync(IDataSource source, [EnumeratorCancellation] CancellationToken ct = default)
    {
        int batch = 0;
        while (!ct.IsCancellationRequested)
        {
            var batchResult = await source.FetchBatchAsync(batch, ct).ConfigureAwait(false);
            if (batchResult.Count == 0) break;

            foreach (var item in batchResult) yield return item;

            batch++;
        }
    }
}