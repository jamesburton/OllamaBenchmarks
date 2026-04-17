using Microsoft.AspNetCore.Components;

public class PaginationComponent : ComponentBase
{
    [Parameter]
    public int TotalItems { get; set; } = 0;

    [Parameter]
    public int PageSize { get; set; } = 10;

    public int CurrentPage { get; private set; } = 1;

    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    protected void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
        }
    }

    protected void PrevPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }
}