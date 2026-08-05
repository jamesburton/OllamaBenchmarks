using System.Threading.Channels

class DataPipeline
{
            public DataPipeline(int capacity)
            {
                Writer = Channel.CreateBounded<string>(capacity);
            }

            public ChannelWriter<string> Writer => Writer;

            public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
            {
                return channel.Reader.ReadAllAsync(ct).AsAsyncEnumerable();
            }

            public Task CompleteAsync()
            {
                Writer.CompletedTask?.Invoke null);
                return Task.CompletedTask;
            }
        }