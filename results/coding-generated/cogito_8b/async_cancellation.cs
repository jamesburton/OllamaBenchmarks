using System;
using System.Collections.Generic;
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
        for (int batchNumber = 0; ; batchNumber++)
        {
            if (ct.IsCancellationRequested)
                yield break;

            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

            if (!batch.Any())
                break;

            foreach (var item in batch)
            {
                yield return item;
            }
        }
    }
}