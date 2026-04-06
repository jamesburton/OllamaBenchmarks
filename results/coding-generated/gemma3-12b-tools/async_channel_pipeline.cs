using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> channel;

    public DataPipeline(int capacity)
    {
        channel = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer { get; } = channel.Writer;

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return channel.Reader.ReadAllAsync(ct);
    }

    public Task CompleteAsync()
    {
        Writer.Complete();
        return Task.CompletedTask;
    }
}