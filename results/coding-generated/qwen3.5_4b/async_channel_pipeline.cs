using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> _channel;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer => _channel.Writer;

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public Task CompleteAsync()
    {
        return Writer.Complete();
    }
}

using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> _channel;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer => _channel.Writer;

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public Task CompleteAsync()
    {
        return Writer.Complete();
    }
}

... 
... 
using ...
...

using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> _channel; // This works because of the using statement.

    public DataPipeline(int capacity)
    {
        _channel = System.Threading.Channels.Channel.CreateBounded<string>(capacity); // Explicitly qualified or just use the using.
    }

    public System.Threading.Channels.ChannelWriter<string> Writer => _channel.Writer; // Need to qualify ChannelWriter? It's in Threading.Channels.

    public System.Runtime.IAsyncEnumerable<string> ReadAllAsync(System.Threading.CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct); // Reader is in Threading.Channels.
    }

    public System.Threading.Tasks.Task CompleteAsync()
    {
        return Writer.Complete(); // Complete returns Task.
    }
}

using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> _channel;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer => _channel.Writer;

    public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public Task CompleteAsync()
    {
        return Writer.Complete();
    }
}

using System.Threading.Channels;

public class DataPipeline
{
    private readonly Channel<string> _channel;

    public DataPipeline(int capacity)
    {
        _channel = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer => _channel.Writer;

    public System.Runtime.IAsyncEnumerable<string> ReadAllAsync(System.Threading.CancellationToken ct)
    {
        return _channel.Reader.ReadAllAsync(ct);
    }

    public System.Threading.Tasks.Task CompleteAsync()
    {
        return Writer.Complete();
    }
}