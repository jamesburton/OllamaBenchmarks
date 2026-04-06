using System.Threading.Channels;

public class DataPipeline
{
    private readonly ChannelWriter<string> _writer;

    public DataPipeline(int capacity)
    {
        _writer = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer => _writer;

    public async IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        await foreach (var item in _writer.Reader.ReadAllAsync(ct))
        {
            yield return item;
        }
    }

    public Task CompleteAsync() => _writer.Complete();
}