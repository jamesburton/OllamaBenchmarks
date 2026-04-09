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
                    // Requirement: Check cancellation BEFORE calling FetchBatchAsync
                    if (ct.IsCancellationRequested)
                    {
                        yield break;
                    }

                    var batch = await source.FetchBatchAsync(batchNumber, ct);
                    ConfigureAwait(false);

                    // Requirement: Break if empty
                    if (batch == null || batch.Count == 0)
                    {
                        break;
                    }

                    foreach (var item in batch)
                    {
                        yield return item;
                    }

                    batchNumber++;
                }
            }
        }

int batchNumber = 0;
    while (true)
    {
        // Check cancellation BEFORE calling FetchBatchAsync
        if (ct.IsCancellationRequested)
        {
            yield break;
        }

        var batch = await source.FetchBatchAsync(batchNumber, ct);
        ConfigureAwait(false);

        if (batch == null || batch.Count == 0)
        {
            break;
        }

        foreach (var item in batch)
        {
            yield return item;
        }

        batchNumber++;
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
                // Check cancellation BEFORE calling FetchBatchAsync
                if (ct.IsCancellationRequested)
                {
                    yield break;
                }

                var batch = await source.FetchBatchAsync(batchNumber, ct);
                ConfigureAwait(false);

                if (batch == null || batch.Count == 0)
                {
                    break;
                }

                foreach (var item in batch)
                {
                    yield return item;
                }

                batchNumber++;
            }
        }
    }

while (true)
    {
        if (ct.IsCancellationRequested)
        {
            yield break;
        }

        var batch = await source.FetchBatchAsync(batchNumber, ct);
        ConfigureAwait(false);

        if (batch == null || batch.Count == 0)
        {
            break;
        }

        try
        {
            foreach (var item in batch)
            {
                yield return item;
            }
        }
        catch (OperationCanceledException)
        {
            yield break;
        }

        batchNumber++;
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
            // Check cancellation BEFORE calling FetchBatchAsync
            if (ct.IsCancellationRequested)
            {
                yield break;
            }

            var batch = await source.FetchBatchAsync(batchNumber, ct);
            ConfigureAwait(false);

            if (batch == null || batch.Count == 0)
            {
                break;
            }

            try
            {
                foreach (var item in batch)
                {
                    yield return item;
                }
            }
            catch (OperationCanceledException)
            {
                yield break;
            }

            batchNumber++;
        }
    }
}