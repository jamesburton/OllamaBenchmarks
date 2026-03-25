using Microsoft.AspNetCore.Components;

namespace MyBlazorApp.Components
{
    public partial class CounterBase : ComponentBase
    {
        [Parameter] public int InitialCount { get; set; }
        [Parameter] public EventCallback<int> OnCountChanged { get; set; }

        private int currentCount;

        protected override void OnInitialized()
        {
            currentCount = InitialCount;
        }

        public void IncrementCount()
        {
            currentCount++;
            OnCountChanged.InvokeAsync(currentCount);
        }

        public int CurrentCount => currentCount;
    }
}