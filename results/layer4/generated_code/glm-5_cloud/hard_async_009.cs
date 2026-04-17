using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class AsyncBatchProcessor<TInput, TOutput>
{
    private readonly int _batchSize;
    private readonly Func<IReadOnlyList<TInput>, CancellationToken, Task<IReadOnlyList<TOutput>>> _batchHandler;

    public AsyncBatchProcessor(int batchSize, Func<IReadOnlyList<TInput>, CancellationToken, Task<IReadOnlyList<TOutput>>> batchHandler)
    {
        if (batchSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be greater than or equal to 1.");
        }

        _batchSize = batchSize;
        _batchHandler = batchHandler ?? throw new ArgumentNullException(nameof(batchHandler));
    }

    public async Task<IReadOnlyList<TOutput>> ProcessAsync(IEnumerable<TInput> items, CancellationToken ct)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var results = new List<TOutput>();

        // Buffer the items to allow for safe multiple enumeration or single-pass processing
        var batch = new List<TInput>(_batchSize);

        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            batch.Add(item);

            if (batch.Count == _batchSize)
            {
                var batchResults = await _batchHandler(batch, ct).ConfigureAwait(false);
                results.AddRange(batchResults);
                batch.Clear();
            }
        }

        // Process remaining items in the final partial batch
        if (batch.Count > 0)
        {
            ct.ThrowIfCancellationRequested();
            var batchResults = await _batchHandler(batch, ct).ConfigureAwait(false);
            results.AddRange(batchResults);
        }

        return results;
    }

    public async Task<IReadOnlyList<TOutput>> ProcessParallelAsync(IEnumerable<TInput> items, int maxDegreeOfParallelism, CancellationToken ct)
    {
        if (items == null)
        {
            throw new ArgumentNullException(nameof(items));
        }
        if (maxDegreeOfParallelism < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism), "Max degree of parallelism must be greater than or equal to 1.");
        }

        // Materialize the input to a list to safely split into batches by index
        var sourceItems = items.ToList();
        var totalItems = sourceItems.Count;

        if (totalItems == 0)
        {
            return Array.Empty<TOutput>();
        }

        // Calculate the number of batches
        int batchCount = (totalItems + _batchSize - 1) / _batchSize;
        var results = new IReadOnlyList<TOutput>[batchCount];

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
            CancellationToken = ct
        };

        try
        {
            await Task.Run(() =>
            {
                Parallel.For(0, batchCount, options, batchIndex =>
                {
                    int skip = batchIndex * _batchSize;
                    int take = Math.Min(_batchSize, totalItems - skip);

                    // Create the batch slice
                    var batchItems = sourceItems.Skip(skip).Take(take).ToList();

                    // Execute the handler synchronously inside the Parallel loop, 
                    // then wait for the result to ensure exceptions are captured correctly in the loop.
                    var batchResult = _batchHandler(batchItems, ct).GetAwaiter().GetResult();

                    results[batchIndex] = batchResult;
                });
            }, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Rethrow cancellation
            throw;
        }
        catch (AggregateException ae)
        {
            // Unwrap aggregate exceptions from Parallel.For
            throw ae.Flatten();
        }

        // Flatten results maintaining original order
        return results.SelectMany(r => r).ToList();
    }
}