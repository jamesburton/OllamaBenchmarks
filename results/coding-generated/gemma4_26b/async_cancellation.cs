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
                // 1. Check cancellation BEFORE calling FetchBatchAsync
                if (ct.IsCancellationRequested)
                {
                    yield break;
                }

                // 2. Fetch batch
                var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

                // 3. Break if empty
                if (batch == null || batch.Count == 0)
                {
                    yield break;
                }

                // 4. Yield items
                foreach (var item in batch)
                {
                    yield return item;
                }

                batchNumber++;
            }
        }
    }

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
                if (ct.IsCancellationRequested)
                {
                    yield break;
                }

                var batch = await source.FetchBatch . . .

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
                if (ct.IsCancellationRequested)
                {
                    yield break;
                }

                var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

                if (batch.Count == 0)
                {
                    yield break;
                }

                foreach (var item in batch)
                {
                    yield return item;
                }

                batchNumber++;
            }
        }
    }

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
                if (ct.IsCancellationRequested)
                {
                    yield break;
                }

                var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

                if (batch.Count == 0)
                {
                    yield break;
                }

                foreach (var item in batch)
                {
                    yield return item;
                }

                batchNumber++;
            }
        }
    }