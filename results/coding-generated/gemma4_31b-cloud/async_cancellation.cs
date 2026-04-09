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
        int currentBatch = 0;

        while (true)
        {
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            var batch = await source.FetchBatchAsync(currentBatch, ct).ConfigureAwait(false);

            if (batch == null || batch.Count == 0)
            {
                break;
            }

            foreach (var item in batch)
            {
                yield return item;
            }

            currentBatch++;
        }
    }
}