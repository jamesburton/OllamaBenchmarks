using System.Runtime.CompilerServices;
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
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            IReadOnlyList<string> batch;
            try
            {
                batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation gracefully.  Don't re-throw.
                yield break;
            }

            if (batch.Count == 0)
            {
                break;
            }

            foreach (string item in batch)
            {
                yield return item;
            }

            batchNumber++;
        }
    }
}