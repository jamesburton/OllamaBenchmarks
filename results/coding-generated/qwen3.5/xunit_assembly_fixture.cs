using System.Threading;

public class SharedCounter : IAsyncLifetime
        {
            private int counter;

            public ValueTask InitializeAsync()
            {
                counter = 0;
                return default;
            }

            public ValueTask DisposeAsync()
            {
                return default;
            }

            public int IncrementAndGet()
            {
                return Interlocked.Increment(ref counter);
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

using System.Threading;

    public class SharedCounter : IAsyncLifetime
    {
        private int counter;

        public ValueTask InitializeAsync()
        {
            counter = 0;
            return default;
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }

        public int IncrementAndGet()
        {
            return Interlocked.Increment(ref counter);
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