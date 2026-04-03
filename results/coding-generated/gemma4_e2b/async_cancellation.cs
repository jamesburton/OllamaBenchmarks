using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

// 1. Interface IDataSource
public interface IDataSource
{
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
}

// 2. Class DataProcessor
public class DataProcessor
{
    /// <summary>
    /// Processes all data by fetching batches from the source.
    /// </summary>
    /// <param name="source">The data source to fetch batches from.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An asynchronous sequence of strings from all batches.</returns>
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
                // Gracefully stop enumeration without throwing an exception
                yield break;
            }

            // 2. Fetch batch
            // Use ConfigureAwait(false) as required
            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

            // 3. Stop if empty
            if (batch.Count == 0)
            {
                break;
            }

            // 4. Yield each item from the batch
            foreach (var item in batch)
            {
                // Check cancellation again before yielding the next item
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

// Example usage (Optional, for context, not required by prompt)
/*
public class MockDataSource : IDataSource
{
    private readonly int totalBatches = 5;

    public Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct)
    {
        // Simulate network delay
        return Task.Delay(100, ct).ContinueWith(_ =>
        {
            if (ct.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }

            if (batchNumber >= totalBatches)
            {
                return new List<string>(); // Stop condition
            }

            // Simulate data
            var data = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                data.Add($"Item_{batchNumber}_{i}");
            }
            return data;
        });
    }
}

public class Program
{
    public static async Task Main()
    {
        var source = new MockDataSource();
        var processor = new DataProcessor();

        // Use a token that can be cancelled
        using var cts = new CancellationTokenSource();

        Console.WriteLine("Starting data processing...");

        await foreach (var item in processor.ProcessAllBatchesAsync(source, cts.Token))
        {
            Console.WriteLine($"Received: {item}");
        }

        Console.WriteLine("Processing finished.");
    }
}
*/