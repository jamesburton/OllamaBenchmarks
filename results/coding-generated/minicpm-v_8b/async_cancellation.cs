using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataProcessor
{
    public interface IDataSource
    {
        Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
    }

    public class DataProcessor
    {
        private readonly IDataSource _dataSource;

        public DataProcessor(IDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async IAsyncEnumerable<string> ProcessAllBatchesAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            while (true)
            {
                try
                {
                    // Check cancellation before fetching batch to avoid unnecessary work if cancelled.
                    await Task.Run(() => _dataSource.FetchBatchAsync(0, ct)).ConfigureAwait(false);

                    var batch = await _dataSource.FetchBatchAsync(0, ct).ConfigureAwait(false);

                    if (batch.Count == 0)
                        break;

                    foreach (var item in batch)
                        yield return item;
                }
                catch (OperationCanceledException ex)
                {
                    // If cancellation is requested while yielding items from a batch,
                    // the next batch will never be fetched and enumeration stops gracefully.
                    ct.ThrowIfCancellationRequested();
                }
            }
        }
    }
}