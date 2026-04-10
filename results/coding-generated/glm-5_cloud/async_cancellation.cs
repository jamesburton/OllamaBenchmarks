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
            // 1. Check cancellation BEFORE fetching
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            // 2. Fetch batch
            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

            // 3. Break if empty
            if (batch.Count == 0)
            {
                break;
            }

            // 4. Yield items
            foreach (var item in batch)
            {
                yield return item;
            }

            batchNumber++;
        }
    }
}