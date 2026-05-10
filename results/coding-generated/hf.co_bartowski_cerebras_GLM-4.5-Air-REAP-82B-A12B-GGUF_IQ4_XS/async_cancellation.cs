using System.Runtime.CompilerServices;

interface IDataSource
{
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
}

class DataProcessor
{
    public async IAsyncEnumerable<string> ProcessAllBatchesAsync(
        IDataSource source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        int batchNumber = 0;

        while (true)
        {
            // Check cancellation before fetching next batch
            if (ct.IsCancellationRequested) yield break;

            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

            if (batch.Count == 0) break;

            foreach (var item in batch)
            {
                await Task.Yield(); // Allow cancellation between items
                yield return item;
            }

            batchNumber++;
        }
    }
}