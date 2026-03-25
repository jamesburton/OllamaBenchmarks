using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

[assembly: AssemblyFixture(typeof(SharedCounter))]

namespace SharedCounterTests
{
    public class SharedCounter : IAsyncLifetime
    {
        private int _counter;

        public ValueTask InitializeAsync()
        {
            _counter = 0;
            return default;
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref _counter);
        }
    }

    public class FirstCounterTests
    {
        private readonly SharedCounter _counter;

        public FirstCounterTests(SharedCounter counter)
        {
            _counter = counter;
        }

        [Fact]
        public void Increment_ReturnsPositive()
        {
            var result = _counter.IncrementAndGet();
            Assert.True(result > 0);
        }
    }

    public class SecondCounterTests
    {
        [Fact]
        public void Increment_ReturnsPositive()
        {
            var fixture = TestContext.Current.GetFixture<SharedCounter>();
            var result = fixture.IncrementAndGet();
            Assert.True(result > 0);
        }
    }
}