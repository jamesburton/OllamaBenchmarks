using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace BlazorApp
{
    public class CounterBase : ComponentBase
    {
        [Parameter]
        public int InitialCount { get; set; }

        [Parameter]
        public EventCallback<int> OnCountChanged { get; set; }

        private int currentCount;

        protected override Task OnParametersSetAsync()
        {
            currentCount = InitialCount;
            return base.OnParametersSetAsync();
        }

        public async Task IncrementCount()
        {
            currentCount++;
            await OnCountChanged.InvokeAsync(currentCount);
        }

        public int CurrentCount => currentCount;
    }
}