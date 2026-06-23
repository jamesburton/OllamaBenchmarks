public class CounterBase : Microsoft.AspNetCore.ComponentBase
{
    [Parameter] public int InitialCount { get; set; }
    [Parameter] public EventCallback<int> OnCountChanged { get; set; }

    private int currentCount = InitialCount;

    protected override async void OnInitialized()
    {
        if (OnCountChanged != null)
            await OnCountChanged.InvokeAsync(InitialCount);
    }

    public async Task IncrementCount() => await OnCountChanged.InvokeAsync(currentCount + 1);

    public int CurrentCount
    {
        private int currentCount;
        public int CurrentCount(string? value) => value ?? throw new ArgumentNullException(nameof(value));
        public int CurrentCount(int value) => value;
    }
}