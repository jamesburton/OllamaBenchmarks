using System.Threading.Channels;

public class DataPipeline
{
    private readonly ChannelWriter<string> _writer;
    public ChannelWriter<string> Writer => _writer;

    public DataPipeline(int capacity)
    {
        var channel = Channel.CreateBounded<string>(capacity);
        _writer = channel.Writer;
    }

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
        => _writer.ToStringAsync(ct);

    public Task CompleteAsync()
        => _writer.CompleteAsync();
}