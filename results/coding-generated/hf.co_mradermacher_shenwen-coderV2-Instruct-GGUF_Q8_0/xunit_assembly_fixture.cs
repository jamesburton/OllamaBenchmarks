using System.Threading.Tasks;
using AwesomeAssertions;

namespace xUnit.v3
{
    public class DatabaseFixture : IAsyncLifetime
    {
        private int _counter = 0;
        private readonly TaskCompletionSource<int> _taskCompletionSource = new TaskCompletionSource<int>();

        public async Task InitializeAsync()
        {
            await _taskCompletionSource.Task;
            Interlocked.Increment(ref _counter);
        }

        public async Task DisposeAsync()
        {
            await _taskCompletionSource.Task;
        }

        [Fact]
        public void IncrementAndGet()
        {
            var counter = _counter;
            Assert.Equal(counter + 1, await _taskCompletionSource.Task);
        }
    }
}