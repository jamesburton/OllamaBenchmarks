using System.Threading.Channels;

public class DataPipeline
{
    public ChannelWriter<string> Writer { get; }

    public DataPipeline(int capacity)
    {
        var channel = Channel.CreateBounded<string>(capacity);
        Writer = channel.Writer;
        _channel = channel;
    }

    private Channel<string> _channel;

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public Task CompleteAsync()
    {
        Writer.Complete();
        return Task.CompletedTask;
    }
}