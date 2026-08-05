using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

public interface IDataSource
{
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);

public class DataProcessor
{
    private readonly IDataSource source;

    public DataProcessor(IDataSource source)
    {
        this.source = source;
    }

    public async Task<IEnumerator<string>> ProcessAllBatchesAsync(
        CancellationToken ct = default)
    {
        while (true)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            Task<IReadOnlyList<string>> batch = await source.FetchBatchAsync(batchNumber, ct);
            if (batch == null)
            {
                break;
            }

            if (batch.Count == 0)
            {
                break;
            }

            yield return batch;
        }
    }
}