using Microsoft.AspNetCore.Components;

public class SearchBoxComponent : ComponentBase
{
    [Parameter]
    public string Placeholder { get; set; } = "Search...";

    protected string Query { get; set; } = "";

    protected bool HasQuery => !string.IsNullOrWhiteSpace(Query);

    protected void Clear()
    {
        Query = "";
    }
}