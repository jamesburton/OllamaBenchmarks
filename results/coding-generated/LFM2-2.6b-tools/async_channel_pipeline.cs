using System.Threading.Channels;

public class DataPipeline


    private readonly Channel<string> _channel;
    public ChannelWriter<string> Writer => _channel.Writer;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    public async IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
        {
            yield return item;
        }
    }

    public Task CompleteAsync()
    {
        return Task.FromResult<Task>(Writer.Complete());
    }