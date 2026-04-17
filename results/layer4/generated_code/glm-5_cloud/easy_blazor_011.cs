using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace BlazorApp.Components
{
    public class TabsComponent : ComponentBase
    {
        [Parameter]
        public List<string> TabNames { get; set; } = new();

        public string? ActiveTab { get; private set; }

        protected override void OnParametersSet()
        {
            // Set ActiveTab to the first tab if it hasn't been set yet and the list is not empty
            if (ActiveTab == null && TabNames != null && TabNames.Count > 0)
            {
                ActiveTab = TabNames[0];
            }
        }

        public void SelectTab(string name)
        {
            ActiveTab = name;
        }
    }
}