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
        int batchNumber = 0;

        while (true)
        {
            // 1. Check cancellation before fetching the next batch
            if (ct.IsCancellationRequested)
            {
                yield break; // Graceful exit
            }

            // 2. Fetch batch
            // We pass the cancellation token to the source operation
            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

            // 3. Stop if batch is empty
            if (batch == null || batch.Count == 0)
            {
                break;
            }

            // 4. Yield each item from the batch
            foreach (var item in batch)
            {
                // Check cancellation again before yielding the next item
                if (ct.IsCancellationRequested)
                {
                    yield break;
                }
                yield return item;
            }

            batchNumber++;
        }
    }
}