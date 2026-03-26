using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            // Check cancellation before fetching next batch
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            if (batch.Count == 0)
            {
                break;
            }

            foreach (var item in batch)
            {
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