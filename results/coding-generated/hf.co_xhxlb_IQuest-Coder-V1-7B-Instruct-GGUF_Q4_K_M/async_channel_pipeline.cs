using System.Threading.Channels;
using System.Threading;
using System.Threading.Tasks;

public class DataPipeline
{
    private readonly Channel<string> _channel;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer => _channel.Writer;

    public async IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
        {
            yield return item;
        }
    }

    public async Task CompleteAsync()
    {
        await _channel.Writer.CompleteAsync();
    }
}