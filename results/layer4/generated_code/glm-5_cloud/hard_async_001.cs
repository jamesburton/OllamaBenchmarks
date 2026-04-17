using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class DataProcessor
{
    private readonly Channel<int> _channel;

    public DataProcessor()
    {
        // Create a bounded channel with a capacity of 10.
        // BoundedChannelFullMode.Wait causes the producer to wait when the channel is full
        // until space becomes available, rather than dropping items or throwing.
        var options = new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.Wait
        };

        _channel = Channel.CreateBounded<int>(options);
    }

    /// <summary>
    /// Produces items and writes them to the bounded channel.
    /// Completes the channel writer when finished or if cancellation is requested.
    /// </summary>
    public async Task ProduceAsync(IEnumerable<int> items, CancellationToken ct)
    {
        try
        {
            foreach (int item in items)
            {
                // Check for cancellation before writing to avoid unnecessary work
                ct.ThrowIfCancellationRequested();

                // WriteAsync will wait if the channel is full (capacity 10).
                await _channel.Writer.WriteAsync(item, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Gracefully handle cancellation by allowing flow to finally block
        }
        catch (ChannelClosedException)
        {
            // Channel was closed externally, safe to exit
        }
        finally
        {
            // Signal that no more items will be written.
            // If cancellation was requested, we pass 'true' to allow the consumer
            // to drain existing items or observe the cancellation gracefully.
            // If 'false' were passed, the consumer would throw immediately upon reading.
            _channel.Writer.Complete();
        }
    }

    /// <summary>
    /// Consumes items from the channel, applies a transform, and returns the results.
    /// </summary>
    public async Task<List<int>> ConsumeAsync(Func<int, int> transform, CancellationToken ct)
    {
        var results = new List<int>();

        try
        {
            // WaitToReadAsync returns false when the writer completes and the channel is empty.
            while (await _channel.Reader.WaitToReadAsync(ct))
            {
                // TryRead is efficient as it does not block; we know an item is available.
                if (_channel.Reader.TryRead(out int item))
                {
                    // Apply the transformation
                    results.Add(transform(item));
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Cancellation requested: stop processing and return what we have collected.
        }

        return results;
    }

    /// <summary>
    /// Orchestrates the pipeline by running the producer and consumer concurrently.
    /// </summary>
    public async Task<List<int>> RunPipelineAsync(IEnumerable<int> items, Func<int, int> transform, CancellationToken ct)
    {
        // Start the producer task
        var producerTask = ProduceAsync(items, ct);

        // Start the consumer task
        var consumerTask = ConsumeAsync(transform, ct);

        // Wait for both tasks to complete.
        // We use Task.WhenAll to ensure both tasks are awaited, even if one faults.
        await Task.WhenAll(producerTask, consumerTask);

        // Return the results from the consumer
        return await consumerTask;
    }
}