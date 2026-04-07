using System.Runtime.CompilerServices;

public interface IDataSource
{
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
}

public class DataProcessor
{
    public async IAsyncEnumerable<string> ProcessAllBatchesAsync(
        IDataSource source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        // We'll use ConfigureAwait(false) on all awaits.
        int batchNumber = 0;
        while (true)
        {
            // Check cancellation BEFORE fetching the next batch.
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            // Fetch the batch.
            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            // If the batch is empty, stop.
            if (batch.Count == 0)
            {
                break;
            }

            // Yield each item from the batch.
            foreach (var item in batch)
            {
                yield return item;
            }

            batchNumber++;
        }
    }
}