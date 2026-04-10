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
            // 1. Check cancellation before fetching
            if (ct.IsCancellationRequested)
            {
                // Yield break if cancelled (do NOT throw)
                break;
            }

            // 2. Fetch batch
            IReadOnlyList<string> batch;
            try
            {
                batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // If FetchBatchAsync throws OCE (which it might if it respects ct), we stop.
                break;
            }
            catch (Exception)
            {
                // Handle other potential exceptions during fetching if necessary, 
                // though the requirement focuses on cancellation.
                break;
            }

            // 3. Break if empty
            if (batch.Count == 0)
            {
                break;
            }

            // 4. Yield items from the batch
            foreach (var item in batch)
            {
                // Check cancellation again before yielding the next item in the batch
                if (ct.IsCancellationRequested)
                {
                    // Stop yielding immediately if cancelled
                    break;
                }
                yield return item;
            }

            batchNumber++;
        }
    }
}