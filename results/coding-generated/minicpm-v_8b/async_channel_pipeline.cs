using System.Threading.Channels;

public class DataPipeline
{
    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    private readonly ChannelWriter<string> _channel;

    public ChannelWriter<string> Writer => _channel.Writer;

    public async IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return await _channel.Reader.ReadAllAsync(ct).ConfigureAwait(false);
    }

    public Task CompleteAsync()
    {
        return _channel.Writer.Complete();
    }
}