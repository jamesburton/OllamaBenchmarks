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

    public System.Collections.Generic.IAsyncEnumerable<string> ReadAllAsync(System.Threading.CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public System.Threading.Tasks.Task CompleteAsync()
    {
        Writer.Complete();
        return System.Threading.Tasks.Task.CompletedTask;
    }
}