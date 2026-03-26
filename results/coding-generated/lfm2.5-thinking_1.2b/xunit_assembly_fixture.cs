public class SharedCounter : IAsyncLifetime
{
    private int counter;
    private ValueTask<int> valueTask;

    public SharedCounter() => counter = 0;
    public ValueTask<int> InitializeAsync() => valueTask = ValueTask.Empty;
    public void DisposeAsync() => valueTask.Dispose();
    public void IncrementAndGet() => Interlocked.Increment(ref counter, 1);
}

[Fact] private SharedCounter counter = new SharedCounter();
[Theory] public void FirstTest() => Assert.Equal(1, counter.IncrementAndGet());
[Test] public class SecondTest
{
    public void SecondTest() => Assert.Equal(1, SharedCounter.IncrementAndGet());
}