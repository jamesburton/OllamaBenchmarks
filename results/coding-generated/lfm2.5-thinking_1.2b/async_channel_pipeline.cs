using System.Threading.Channels;

public class DataPipeline
{
    public ChannelWriter<string> Writer { get; }
    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return Channel.CreateBounded<string>(capacity).Reader.ReadAllAsync(ct);
    }
    public async Task CompleteAsync()
    {
        await Writer.Complete();
    }
}