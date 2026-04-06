using System.Threading.Channels;

class DataPipeline
{
    private readonly Channel<string> _channel;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer => _channel.Writer;

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);

    public Task CompleteAsync()
    {
        Writer.Complete();
        return Task.CompletedTask;
    }
}