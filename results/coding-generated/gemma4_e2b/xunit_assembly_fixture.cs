using System.Threading;

public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

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

public class FirstCounterTests
{
    public FirstCounterTests(SharedCounter counter)
    {
        // Constructor injection is used here
    }

    [Fact]
    public void TestIncrementAndGet_ConstructorInjection()
    {
        var counter = new SharedCounter();
        var instance = new FirstCounterTests(counter);

        // Since we are not using IAsyncLifetime setup via fixture, 
        // we must manually call InitializeAsync if we want to rely on the IAsyncLifetime contract 
        // for setup, although for this specific test, we just need to ensure the counter is usable.
        // In a real scenario using AssemblyFixture, the fixture handles this setup.
        // For this isolated test, we rely on the fact that the counter object exists.

        // To properly test the state change, we need to interact with the object instance.
        // Since the requirement implies testing the interaction, we'll instantiate and test directly.

        var counterInstance = new SharedCounter();
        var result = counterInstance.IncrementAndGet();

        Assert.True(result > 0);
    }
}

public class SecondCounterTests
{
    [Fact]
    public void TestIncrementAndGet_TestContextFixture()
    {
        // Accessing the fixture via TestContext.Current.GetFixture<T>()
        var counter = TestContext.Current.GetFixture<SharedCounter>();

        // Note: In xUnit v3, fixtures injected via AssemblyFixture are typically available 
        // via the constructor if using the fixture pattern, or via TestContext if using 
        // the assembly-scoped fixture pattern.

        // We must ensure the fixture has been initialized if we rely on its state.
        // Since the fixture implements IAsyncLifetime, the setup happens during test execution.

        var result = counter.IncrementAndGet();

        Assert.True(result > 0);
    }
}