using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

public class ChecklistComponent : ComponentBase
{
    [Parameter]
    public List<string> Items { get; set; } = new();

    protected HashSet<string> Checked { get; set; } = new();

    protected void ToggleItem(string item)
    {
        if (!Checked.Add(item))
        {
            Checked.Remove(item);
        }
    }

    protected bool IsChecked(string item)
    {
        return Checked.Contains(item);
    }

    protected int CheckedCount => Checked.Count;
}