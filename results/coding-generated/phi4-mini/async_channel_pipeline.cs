using System.Threading.Channels;
using System.Threading.Tasks;

public class DataPipeline<T>
{
    private readonly ChannelWriter<T> _writer;

    public DataPipeline(int capacity)
    {
        _writer = new ChannelWriter<T>(capacity);
    }

    public ChannelWriter<string> Writer => _writer;

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return _writer.Reader.ReadAllAsync(ct).AsAsyncEnumerable();
    }

    public Task CompleteAsync()
    {
        return _writer.Complete().ContinueWith(Task.CompletedTask);
    }
}