using System;
using System.Threading.Channels;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Async;

public class DataPipeline
{
    private readonly Channel<string> _writer;

    public DataPipeline(int capacity)
    {
        _writer = Channel.CreateBounded<string>(capacity);
    }

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return _writer.Reader.ReadAllAsync(ct);
    }

    public Task CompleteAsync()
    {
        return _writer.Writer.Complete();
    }
}