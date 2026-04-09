using System.Runtime.CompilerServices;

public interface IDataSource
{
    System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<string>> FetchBatchAsync(int batchNumber, System.Threading.CancellationToken ct);
}

public class DataProcessor
{
    public async System.Collections.Generic.IAsyncEnumerable<string> ProcessAllBatchesAsync(
        IDataSource source,
        [EnumeratorCancellation] System.Threading.CancellationToken ct = default)
    {
        int batchNumber = 0;
        while (true)
        {
            // Cancellation check before fetching the next batch
            if (ct.IsCancellationRequested)
                yield break;

            var batch = await source
                .FetchBatchAsync(batchNumber, ct)
                .ConfigureAwait(false);

            // Stop when an empty batch is returned
            if (batch == null || batch.Count == 0)
                break;

            // Yield each item from the current batch
            foreach (var item in batch)
            {
                yield return item;
            }

            batchNumber++;
        }
    }
}