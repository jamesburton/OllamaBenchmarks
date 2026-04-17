using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

public class FilterListComponent : ComponentBase
{
    [Parameter]
    public List<string> AllItems { get; set; } = new();

    protected string FilterText { get; set; } = "";

    protected List<string> FilteredItems => AllItems
        .Where(x => x.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
        .ToList();
}