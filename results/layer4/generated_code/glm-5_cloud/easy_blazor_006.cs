using Microsoft.AspNetCore.Components;

public class SearchBoxComponent : ComponentBase
{
    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    protected string SearchText { get; set; } = "";

    protected async Task HandleSearch()
    {
        if (OnSearch.HasDelegate)
        {
            await OnSearch.InvokeAsync(SearchText);
        }
    }
}