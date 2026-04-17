using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

public class TabsComponent : ComponentBase
{
    [Parameter]
    public List<string> Tabs { get; set; } = new();

    public string ActiveTab { get; set; } = "";

    public void SelectTab(string tab)
    {
        ActiveTab = tab;
    }

    public void OnInitializedPublic()
    {
        OnInitialized();
    }

    protected override void OnInitialized()
    {
        ActiveTab = Tabs.FirstOrDefault() ?? "";
    }
}