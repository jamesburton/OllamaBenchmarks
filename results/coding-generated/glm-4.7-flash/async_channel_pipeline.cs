public class DataPipeline
        {
            // Fields needed to store the channel and writer
            private readonly Channel<string> _channel;
            public ChannelWriter<string> Writer { get; }

            // Constructor
            public DataPipeline(int capacity)
            {
                _channel = Channel.CreateBounded<string>(capacity);
                Writer = _channel.Writer;
            }

            // ReadAllAsync method
            public IAsyncEnumerable<string> ReadAllAsync(CancellationToken ct)
            {
                return _channel.Reader.ReadAllAsync(ct);
            }

            // CompleteAsync method
            public Task CompleteAsync()
            {
                Writer.Complete();
                return Task.CompletedTask;
            }
        }

using System.Threading.Channels;

    public class DataPipeline
    {
        private readonly Channel<string> _channel;
        public ChannelWriter<string> Writer { get; }

        public DataPipeline(int capacity)
        {
            _channel = Channel.CreateBounded<string>(capacity);
            Writer = _channel.Writer;
        }

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

using System.Threading.Channels;

    public class DataPipeline
    {
        private readonly Channel<string> _channel;
        public ChannelWriter<string> Writer { get; }

        public DataPipeline(int capacity)
        {
            _channel = Channel.CreateBounded<string>(capacity);
            Writer = _channel.Writer;
        }

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

using System.Threading.Channels;

    public class DataPipeline
    {
        private readonly Channel<string> _channel;
        public ChannelWriter<string> Writer { get; }

        public DataPipeline(int capacity)
        {
            _channel = Channel.CreateBounded<string>(capacity);
            Writer = _channel.Writer;
        }

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

using System.Threading.Channels;

    public class DataPipeline
    {
        private readonly Channel<string> _channel;
        public ChannelWriter<string> Writer { get; }

        public DataPipeline(int capacity)
        {
            _channel = Channel.CreateBounded<string>(capacity);
            Writer = _channel.Writer;
        }

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