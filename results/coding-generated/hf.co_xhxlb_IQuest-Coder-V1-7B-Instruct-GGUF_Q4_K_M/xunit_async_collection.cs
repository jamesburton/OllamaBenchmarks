using System;
using System.Threading.Tasks;
using xunit;
using AwesomeAssertions;
using NSubstitute;

namespace CounterDemo
{
    public class CounterService
    {
        public int Count { get; private set; }

        public void Increment()
        {
            Count++;
        }

        public void Reset()
        {
            Count = 0;
        }
    }

    public class CounterFixture : IAsyncLifetime
    {
        public CounterService Counter { get; private set; } = null!;

        public async ValueTask InitializeAsync()
        {
            Counter = new CounterService();
            await Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await Task.CompletedTask;
        }
    }

    [CollectionDefinition("Counter")]
    public class CounterCollection : ICollectionFixture<CounterFixture>
    {
    }

    public class CounterFirstTests
    {
        private CounterFixture _fixture;

        public CounterFirstTests(CounterFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void IncrementOnce()
        {
            _fixture.Counter.Increment();
            _fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(1);
        }
    }

    public class CounterSecondTests
    {
        private CounterFixture _fixture;

        public CounterSecondTests(CounterFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void IncrementTwice()
        {
            _fixture.Counter.Increment();
            _fixture.Counter.Increment();
            _fixture.Counter.Count.Should().BeGreaterThanOrEqualTo(2);
        }
    }
}