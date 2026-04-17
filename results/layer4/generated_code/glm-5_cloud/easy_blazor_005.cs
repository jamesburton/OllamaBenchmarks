using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LoaderComponent : ComponentBase
{
    public bool IsLoading { get; private set; } = false;
    public List<string> Items { get; private set; } = new();

    public async Task LoadAsync()
    {
        IsLoading = true;
        await Task.Delay(1);
        Items = new List<string> { "Item 1", "Item 2", "Item 3" };
        IsLoading = false;
    }
}