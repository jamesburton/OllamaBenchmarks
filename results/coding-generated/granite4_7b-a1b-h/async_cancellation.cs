using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace DataProcessingNamespace
{
    public interface IDataSource
    {
        Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
    }

    public class DataProcessor
    {
        public async IAsyncEnumerable<string> ProcessAllBatchesAsync(IDataSource source,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            int batchNumber = 0;
            while (true)
            {
                var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
                if (batch == null || batch.Count == 0)
                {
                    break;
                }

                foreach (var item in batch)
                {
                    if (ct.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(ct);
                    }
                    yield return item;
                }

                batchNumber++;
            }
        }
    }
}