public class CounterBase
{
            private int currentCount;
            [Parameter] public int InitialCount { get; set; }
            [Parameter] public EventCallback<int> OnCountChanged { get; set; }

            protected override async Task OnInitialized()
            {
                currentCount = InitialCount;
            }

            public async Task IncrementCount() => await OnCountChanged.InvokeAsync(currentCount + 1);
        }