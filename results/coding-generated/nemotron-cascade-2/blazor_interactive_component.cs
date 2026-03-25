using System;
using Microsoft.AspNetCore.Components;

namespace MyApp.Components
{
    public class CounterBase : ComponentBase
    {
        private int currentCount;

        [Parameter]
        public int InitialCount { get; set; }

        [Parameter]
        public EventCallback<int> OnCountChanged { get; set; }

        protected override void OnInitialized()
        {
            currentCount = InitialCount;
        }

        public void IncrementCount()
        {
            currentCount++;
            // Ensure the event is raised inside the Blazor render context
            _ = OnCountChanged.InvokeAsync(currentCount);
        }

        public int CurrentCount => currentCount;
    }
}