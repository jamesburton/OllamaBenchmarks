public class SharedCounter : IAsyncLifetime
{
    private int counter;

    public ValueTask InitializeAsync()
    {
        counter = 0;
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public int IncrementAndGet()
    {
        return System.Threading.Interlocked.Increment(ref counter);
    }
}

public class FirstCounterTests
{
    public FirstCounterTests(SharedCounter counter)
    {
        // Constructor injection
    }

    [Fact]
    public void Test_IncrementAndGet_StartsAtZero()
    {
        var counter = new SharedCounter();

        // Initialize the fixture
        counter.InitializeAsync().AsTask(); 

        var result = counter.IncrementAndGet();

        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void Test_IncrementAndGet_ViaFixture_StartsAtZero()
    {
        // Retrieve the fixture using TestContext
        var counter = TestContext.Current.GetFixture<SharedCounter>();

        // Initialize the fixture
        counter.InitializeAsync().AsTask();

        var result = counter.IncrementAndGet();

        Assert.True(result > 0);
    }
}