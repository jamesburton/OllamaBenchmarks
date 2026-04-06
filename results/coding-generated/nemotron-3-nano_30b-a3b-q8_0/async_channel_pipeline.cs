using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> _channel;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer => _channel.Writer;

    public async System.Threading.Tasks.IAsyncEnumerable<string> ReadAllAsync(System.Threading.CancellationToken ct)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
            yield return item;
    }

    public System.Threading.Tasks.Task CompleteAsync()
    {
        Writer.Complete();
        return System.Threading.Tasks.Task.CompletedTask;
    }
}