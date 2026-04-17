using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

public class SortableListComponent : ComponentBase
{
    [Parameter]
    public List<string> Items { get; set; } = new List<string>();

    protected bool SortAscending { get; set; } = true;

    protected List<string> SortedItems => SortAscending 
        ? Items.OrderBy(x => x).ToList() 
        : Items.OrderByDescending(x => x).ToList();

    protected void ToggleSort()
    {
        SortAscending = !SortAscending;
    }
}