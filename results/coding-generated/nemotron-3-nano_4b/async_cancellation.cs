using System.Runtime.CompilerServices;

// interface
public interface IDataSource
{
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
}

// processor
public class DataProcessor
{
    public async IAsyncEnumerable<string> ProcessAllBatchesAsync(
        IDataSource source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var batchIdx = 0;
        while (!ct.IsCancellationRequested)
        {
            // cancellation checked BEFORE fetching the batch
            if (ct.IsCancellationRequested)
                break;

            var batch = await source.FetchBatchAsync(batchIdx, ct).ConfigureAwait(false);
            batchIdx++;

            if (batch == null || batch.Count == 0)
                break;

            foreach (var item in batch)
            {
                // cancellation checked WHILE iterating items
                if (ct.IsCancellationRequested)
                    break;
                yield return item;
            }
        }
    }
}