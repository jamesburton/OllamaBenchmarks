public async IAsyncEnumerable<string> ProcessAllBatchsAsync(IDataSource source, [EnumeratorCancellation] CancellationingToken ct = default)
        {
            int batch = 0;
            while (true)
            {
                if (ct.IsCancellationRequested) yield break;
                var items = await source.FetchBatchAsync(batch, ct).ConfigureAwait(false);
                if (items == null || items.Count == 0) break;
                foreach (var item in items) yield return item;
                batch++;
            }
        }