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
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            batchNumber++;

            if (batch.Count == 0)
            {
                yield break;
            }

            foreach (var item in batch)
            {
                yield return item;
            }
        }
    }
}