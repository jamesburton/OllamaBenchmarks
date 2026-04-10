using Microsoft.AspNetCore.Components;

    public class CounterBase : ComponentBase
    {
        [Parameter] 
        public int InitialCount { get; set; }

        [Parameter] 
        public EventCallback<int> OnCountChanged { get; set; }

        private int _currentCount;

        protected override void OnInitialized()
        {
            _currentCount = InitialCount;
s        }

        public async Task IncrementCount()
        {
            _currentCount++;
            await OnCountChanged.InvokeAsync(_currentCount);
        }

        public int CurrentCount => _currentCount;
    }