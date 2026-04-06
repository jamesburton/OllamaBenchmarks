using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> _channel;
    private readonly ChannelWriter<string> _writer;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
        _writer = _channel.Writer;
    }

    public ChannelWriter<string> Writer => _writer;

    public System.Collections.Generic.IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public Task CompleteAsync()
    {
        _writer.Complete();
        return System.Threading.Tasks.Task.CompletedTask;
    }
}