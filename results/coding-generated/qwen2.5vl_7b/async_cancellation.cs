using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            var batch = await source.FetchBatchAsync(batchNumber++, ct);
            if (!batch.Any())
            {
                break;
            }

            foreach (var item in batch)
            {
                yield return item;
            }

            if (ct.IsCancellationRequested)
            {
                throw new OperationCanceledException("Batch processing was cancelled.");
            }
        }
    }
}