using System.Runtime.CompilerServices;

interface IDataSource {
    Task<IReadOnlyList<string>> FetchBatchAsync(int batchNumber, CancellationToken ct);
}

class DataProcessor {
    async IAsyncEnumerable<string> ProcessAllBatchesAsync(
        IDataSource source,
        [EnumeratorCancellation] CancellationToken ct = default) {

        int batchNumber = 0;
        while (true) {
            if (ct.IsCancellationRequested) {
                yield break; // Do NOT throw OperationCanceledException
            }

            IReadOnlyList<string> batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            if (!batch.Any()) {
                break; // Stop when a batch returns empty
            }

            foreach (string item in batch) {
                yield return item;
            }

            batchNumber++;
        }
    }
}