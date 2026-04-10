using System.Threading.Channels;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DataPipeline
{
    private readonly Channel<string> _channel;
    private readonly ChannelWriter<string> _writer;
    private readonly ChannelReader<string> _reader;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
        _writer = _channel.Writer;
        _reader = _channel.Reader;
    }

    public ChannelWriter<string> Writer => _writer;

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return _reader.ReadAllAsync(ct);
    }

    public Task CompleteAsync()
    {
        _writer.Complete();
        return Task.CompletedTask;
    }
}