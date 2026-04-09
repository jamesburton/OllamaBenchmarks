public async IAsyncEnumerable<string> ProcessAllBatchesAsync(
            IDataSource source,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            int batchNumber = 0;
            while (true)
            {
                // Check cancellation before fetching next batch
                if (ct.IsCancellationRequested)
                {
                    yield break;
                }

                var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);

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

while (true) {
            if (ct.IsCancellationRequested) yield break;
            var batch = await source.FetchBatchAsync(batchNumber, ct).ConfigureAwait(false);
            // ...
        }