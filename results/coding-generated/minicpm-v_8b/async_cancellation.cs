using System;
using System.Collections.Generic;
using System.Threading.Tasks;

interface IDataSource
{
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
}

class DataProcessor
{
    async IAsyncEnumerable<string> ProcessAllBatchesAsync(IDataSource source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        int batchNumber = 0;
        while (true)
        {
            try
            {
                var batch = await source.FetchBatchAsync(batchNumber, ct);
                if (batch.Count == 0) break;

                foreach (var item in batch)
                    yield return item;

                batchNumber++;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
        }
    }
}