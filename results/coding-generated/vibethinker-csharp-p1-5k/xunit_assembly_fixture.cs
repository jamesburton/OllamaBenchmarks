public class SharedCounter : IAsyncLifetime
{
    private int counter = 0;

    public ValueTask InitializeAsync() => Task.CompletedTask;
    public ValueTask DisposeAsync() => Task.CompletedTask;

    public void IncrementAndGet() => Interlocked.Increment(counter);
    public int Get() => counter;
}