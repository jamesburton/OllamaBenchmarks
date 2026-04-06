public DataPipeline(int capacity)
        {
            Channel = Channel.CreateBounded<string>(capacity);
        }

public ChannelWriter<string> Writer { get; }

public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
        {
            return Channel.Reader.ReadAllAsync(ct);
        }

public Task CompleteAsync()
        {
            Writer.Complete();
            return Task.CompletedTask;
        }