using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            if (batch.Count == 0)
                break;

            foreach (var item in batch)
            {
                yield return item;
            }

            if (ct.IsCancellationRequested)
            {
                throw new OperationCanceledException(ct);
            }

            batchNumber++;
        }
    }
}