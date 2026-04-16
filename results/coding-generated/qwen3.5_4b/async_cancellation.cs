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
            // Check cancellation before fetching the next batch
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            var batch = await source.FetchBatchAsync(batchNumber, ct);
            ConfigureAwait(false);

            // Stop if the batch is empty
            if (batch.Count == 0)
            {
                break;
            }

            // Yield each item in the batch
            foreach (var item in batch)
            {
                yield return item;
            }

            batchNumber++;
        }
    }
}