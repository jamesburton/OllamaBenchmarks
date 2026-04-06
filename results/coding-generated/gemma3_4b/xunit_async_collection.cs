using AwesomeAssertions;
using Xunit;

namespace SharedTests
{
    [Collection("Counter")]
    public class CounterFixture
    {
        public CounterService Counter { get; private set; } = null!;

        public ValueTask InitializeAsync()
        {
            Counter = new CounterService();
            return default;
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }
    }

    public class CounterService
    {
        public int Count { get; private set; } = 0;

        public void Increment()
        {
            Count++;
        }

        public void Reset()
        {
            Count = 0;
        }
    }

    public class CounterFirstTests : IObjectCollectionFixture<CounterFixture>
    {
        [Fact]
        public void Increment_Counter()
        {
            var fixture = new CounterFixture();
            fixture.InitializeAsync().Wait();

            fixture.Counter.Increment();

            fixture.Counter.Count.Should().Be(1);
        }
    }

    public class CounterSecondTests : IObjectCollectionFixture<CounterFixture>
    {
        [Fact]
        public void Increment_MultipleTimes()
        {
            var fixture = new CounterFixture();
            fixture.InitializeAsync().Wait();

            fixture.Counter.Increment();
            fixture.Counter.Increment();

            fixture.Counter.Count.Should().Be(2);
        }
    }
}