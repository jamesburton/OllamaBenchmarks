public DataPipeline(int capacity)
        {
            Channel = Channel.CreateBounded<string>(capacity);
        }

private Channel<string> _channel;

private Channel<string> _channel;
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

public Task CompleteAsync()
        {
            Writer.Complete();
            return Task.CompletedTask;
        }

using System.Threading.Channels;

        public class DataPipeline
        {
            private Channel<string> _channel;

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
                Writer.Complete();
                return Task.CompletedTask;
            }
        }