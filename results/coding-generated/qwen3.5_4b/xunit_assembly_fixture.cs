using System.Threading; // Allowed per instructions

    public class SharedCounter : IAsyncLifetime
    {
        private int _counter = 0;

        public ValueTask InitializeAsync() => default;
        public ValueTask DisposeAsync() => default;

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref _counter);
        }
    }

    public class FirstCounterTests(SharedCounter counter)
    {
        [Fact]
        public void Test()
        {
            Assert.True(counter.IncrementAndGet() > 0);
        }
    }

    public class SecondCounterTests
    {
        [Fact]
        public void Test()
        {
            var fixture = TestContext.Current.GetFixture<SharedCounter>();
            Assert.True(fixture.IncrementAndGet() > 0);
        }
    }