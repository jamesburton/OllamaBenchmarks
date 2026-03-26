using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
            if (ct.IsCancellationRequested) yield break;

            IReadOnlyList<string> items = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            if (!items.Any()) yield break;

            foreach (var item in items)
            {
                yield return item;
            }

            batchNumber++;
        }
    }
}