using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> _channel;
    public ChannelWriter<string> Writer { get; }

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
        Writer = _channel.Writer;
    }

    public async IAsyncEnumerable<string> ReadAllAsync([EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
        {
            yield return item;
        }
    }

    public Task CompleteAsync()
    {
        Writer.Complete();
        return Task.CompletedTask;
    }
}