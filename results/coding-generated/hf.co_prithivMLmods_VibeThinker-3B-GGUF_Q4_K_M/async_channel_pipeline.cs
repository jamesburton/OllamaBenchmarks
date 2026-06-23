using System.Threading.Channels;

public class DataPipeline
{
    private Channel<string> Writer;

    public DataPipeline(int capacity)
    {
        Writer = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer => Writer;

    public IAsyncEnumerable<string> ReadAllAsync(Tokens cancellationToken)
    {
        return Writer.ReadAllReadAllAsync(cancellationToken);
    }

    public Task CompletedAsync()
    {
        Writer Complete();
        return Task.CompletedTask;
    }
}

using System.Threading.Channels;

public class DataPipeline
{
    private Channel<string> writer;

    public DataPipeline(int capacity)
    {
        writer = Channel.CreateBounded<string>(capacity);
    }

    public ChannelWriter<string> Writer { get { return writer; } }

    public IAsyncEnumerable<string> ReadAllAsync(Tokens cancellationToken)
    {
        return writer.ReadAllReadAllAsync(cancellationToken);
    }

    public Task CompletedAsync()
    {
        writer Complete();
        return Task.CompletedTask;
    }
}

public async Task<IAsyncEnumerable<string>> ReadAllAsync(Tokens ct)

public async Task<IAsyncEnumerable<T>> ReadAllAsync(Tokens ct);

public async Task<IAsyncEnumerable<T>> ReadAllAsync(Tokens ct) on Channel<T>

public async Task<IAsyncEnumerable<T>> ReadAllAsync(Tokens ct)

public async Task<IAsyncEnumerable<T>> ReadAllAsync(Tokens ct)

return writer.Reader.ReadAllAsync(cancellationToken);

return writer.Reader reads all async ct;

return writer.Reader readsAllAsync(cancellationToken);

public async Task<IAsyncEnumerable<T>> ReadAllAsync(Tokens ct)

public async Task<IAsyncEnumerable<T>> ReadAllAsync(Tokens ct)

var result = channel.Reader readsAllAsync(ct);

var result = channel.Reader readsAllAsync(ct);

var result = channel.Reader readsAllAsync(ct);

await channel.Reader readsAllAsync(ct);

public async Task<IAsyncEnumerable<T>> ReadAllAsync(Tokens ct)

return writer.Reader readsAllAsync(cancellationToken);

return writer.Reader readsAllAsync(cancellationToken);

private System.Threading.Channels_channel<string> writer;