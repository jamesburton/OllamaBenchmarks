using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> _channel;
    public ChannelWriter<string> Writer => _channel.Writer;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public Task CompleteAsync()
    {
        _channel.Writer.Complete();
        return Task.CompletedTask;
    }
}