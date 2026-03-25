using System;
using System.Threading.Tasks;

namespace SharedCounter
{
    public class SharedCounter : IAsyncLifetime
    {
        private int _counter = 0;

        public ValueTask InitializeAsync()
        {
            Interlocked.Increment(ref _counter);
            return default;
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }

        public int IncrementAndGet()
        {
            var result = Interlocked.Increment(ref _counter);
            if (result == 0)
                throw new InvalidOperationException("Counter is zero");
            return result;
        }
    }
}

namespace FirstCounterTests
{
    using System.Threading.Tasks;

    [Fact]
    public async Task Test1()
    {
        // Inject SharedCounter via constructor or use Fixture
        var counter = await TestContext.Current.GetFixture<SharedCounter>().IncrementAndGet();
        Assert.True(counter > 0);
    }
}

namespace SecondCounterTests
{
    [Fact]
    public void Test2()
    {
        // Use TestContext to get the fixture instead of constructor injection
        var counter = TestContext.Current.GetFixture<SharedCounter>().IncrementAndGet();
        Assert.True(counter > 0);
    }
}