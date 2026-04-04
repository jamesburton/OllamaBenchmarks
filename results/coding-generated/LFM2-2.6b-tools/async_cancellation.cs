public interface IDataSource
{
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
}

public class DataProcessor
{
    public async IAsyncEnumerable<string> ProcessAllBatchesAsync(IDataSource source, CancellationToken ct = default)
    {
        int batchNumber = 0;
        while (true)
        {
            if (ct.IsCancellationRequested) yield break;
            if (await FetchBatchWithCheckAsync(source, batchNumber, ct)) yield break;
            if (await FetchBatchWithCheckAsync(source, batchNumber + 1, ct)) continue;
            if (await FetchBatchWithCheckAsync(source, batchNumber + 1, ct)?.Any() == false) break;
            foreach (var item in await FetchBatchWithCheckAsync(source, batchNumber + 1, ct)) yield return item;
            batchNumber++;
        }
    }

    private async Task<IReadOnlyList<string>> FetchBatchWithCheckAsync(IDataSource source, int batchNumber, CancellationToken ct)
    {
        if (ct.IsCancellationRequested) return new List<string>();
        var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
        if (batch.Count == 0) return new List<string>();
        return batch;
    }
}