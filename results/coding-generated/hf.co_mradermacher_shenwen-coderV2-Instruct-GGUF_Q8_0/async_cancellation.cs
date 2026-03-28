using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDataSource
{
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
}

public class DataProcessor : IAsyncEnumerable<string>
{
    private readonly IDataSource _source;
    private readonly CancellationToken _cancellationToken = default(CancellationToken);

    public DataProcessor(IDataSource source)
    {
        _source = source;
    }

    public async IAsyncEnumerable<string> ProcessAllBatchesAsync(
        IDataSource source,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        while (true)
        {
            try
            {
                var batchResult = await _source.FetchBatchAsync(0, ct);
                if (!batchResult.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Failed to fetch batch: {batchResult.StatusCode}");
                }

                foreach (var item in batchResult.Value)
                {
                    yield return item;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (TaskCanceledException)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }

    public async Task<IReturnValue> ConfigureAwait(bool awaitAnyRemainingTasks = true)
    {
        return await _source.ConfigureAwait(false);
    }
}