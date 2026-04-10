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
            // 1. Check cancellation BEFORE fetching the next batch
            if (ct.IsCancellationRequested)
            {
                // Yield break if cancelled, do NOT throw OperationCanceledException
                yield break;
            }

            // 2. Fetch batch
            IReadOnlyList<string> batch;
            try
            {
                batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // If cancellation happens during the await, we stop gracefully.
                yield break;
            }
            catch
            {
                // Handle other potential exceptions if necessary, but for this scope, we just stop.
                yield break;
            }

            // 3. Break if empty
            if (batch == null || batch.Count == 0)
            {
                break;
            }

            // 4. Yield items
            foreach (var item in batch)
            {
                // Check cancellation before yielding the next item (optional, but good practice)
                if (ct.IsCancellationRequested)
                {
                    yield break;
                }
                yield return item;
            }

            batchNumber++;
        }
    }
}