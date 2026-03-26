using System.Runtime.CompilerServices;

interface IDataSource
{
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
}

class DataProcessor
{
    async IAsyncEnumerable<string> ProcessAllBatchesAsync(
        IDataSource source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        for (int i = 0; ; i++)
        {
            if (ct.IsCancellationRequested) yield break;

            var batch = await source.FetchBatchAsync(i, ct).ConfigureAwait(false);

            if (!batch.Any()) break;

            foreach (var item in batch)
            {
                yield return item;
            }
        }
    }
}