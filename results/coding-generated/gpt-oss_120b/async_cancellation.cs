using System.Runtime.CompilerServices;

public interface IDataSource
{
    System.Threading.Tasks.Task<System.Collections.Generic.IReadOnlyList<string>> FetchBatchAsync(
        int batchNumber,
        System.Threading.CancellationToken ct);
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
            if (ct.IsCancellationRequested)
                yield break;

            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            if (batch == null || batch.Count == 0)
                break;

            foreach (var item in batch)
            {
                yield return item;
            }

            batchNumber++;
        }
    }
}