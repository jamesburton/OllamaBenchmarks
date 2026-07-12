public class CounterBase : ComponentBase {
    public int InitialCount { get; set; } = 0;
    public EventCallback<int> OnCountChanged { get; set; } = null;

    private int currentCount = InitialCount;

    public int CurrentCount { get; } { get { return currentCount; } }

    public Task IncrementCount() {
        return OnCountChanged.InvokeAsync(currentCount++);
    }
}