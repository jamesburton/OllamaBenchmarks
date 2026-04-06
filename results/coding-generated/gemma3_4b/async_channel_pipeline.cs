using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> _channel;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer { get; } = _channel.Writer;

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public Task CompleteAsync()
    {
        return Writer.Complete();
    }
}