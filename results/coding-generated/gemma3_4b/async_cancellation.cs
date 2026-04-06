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
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            try
            {
                var batch = await source.FetchBatchAsync(batchNumber, ct);

                if (batch == null || batch.Count == 0)
                {
                    yield break;
                }

                foreach (string item in batch)
                {
                    yield return item;
                }
            }
            catch (OperationCanceledException)
            {
                yield break;
            }

            batchNumber++;
        }
    }
}