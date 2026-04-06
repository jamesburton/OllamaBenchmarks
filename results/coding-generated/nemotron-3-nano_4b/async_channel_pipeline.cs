using System.Threading.Channels;

class DataPipeline
{
    private readonly Channel<string> _channel;
    private readonly ChannelWriter<string> _writer;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
        _writer = _channel.Writer;
    }

    public ChannelWriter<string> Writer => _writer;

    public async IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync<string>(ct);

    public async Task CompleteAsync() => await Writer.CompleteAsync();
}