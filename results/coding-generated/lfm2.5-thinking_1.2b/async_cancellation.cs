using System.Runtime.CompilerServices;

public class DataProcessor
{
    public async IAsyncEnumerable<string> ProcessAllBatchesAsync(IDataSource source, [EnumeratorCancellation] CancellationToken ct = default)
    {
        int i = 0;
        while (true)
        {
            if (ct.IsCancellationRequested)
            {
                yield break;
            }
            var batch = await source.FetchBatchAsync(i, ct);
            if (batch == null || batch.Count == 0)
            {
                yield break;
            }
            yield return batch;
            i++;
        }
    }
}