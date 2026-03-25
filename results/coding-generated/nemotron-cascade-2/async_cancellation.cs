using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace MyNamespace
{
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
                // Fetch the next batch
                IReadOnlyList<string> batch = await source.FetchBatchAsync(batchNumber, ct)
                                                         .ConfigureAwait(false);

                // Stop when a batch returns empty
                if (batch.Count == 0)
                {
                    break;
                }

                // Yield each item from the batch
                foreach (var item in batch)
                {
                    yield return item;
                }

                // Check for cancellation before fetching the next batch
                if (ct.IsCancellationRequested)
                {
                    throw new OperationCanceledException(ct);
                }

                batchNumber++;
            }
        }
    }
}