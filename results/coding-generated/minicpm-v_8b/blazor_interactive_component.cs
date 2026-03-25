using Microsoft.AspNetCore.Components;

public partial class CounterBase : ComponentBase
{
    [Parameter] public int InitialCount { get; set; }
    [Parameter] public EventCallback<int> OnCountChanged { get; set; }

    private int _currentCount = 0;

    protected override void OnInitialized()
    {
        _currentCount = InitialCount;
    }

    public int CurrentCount
    {
        get => _currentCount;
        private set => SetProperty(ref _currentCount, value);
    }

    public async Task IncrementCount()
    {
        await StateHasChangedAsync();
        if (OnCountChanged != null)
            OnCountChanged.InvokeAsync(CurrentCount + 1);
    }
}