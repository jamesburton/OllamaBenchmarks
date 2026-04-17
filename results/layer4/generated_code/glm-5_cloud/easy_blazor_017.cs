using Microsoft.AspNetCore.Components;

public class PaginationComponent : ComponentBase
{
    [Parameter]
    public int TotalItems { get; set; }

    [Parameter]
    public int PageSize { get; set; } = 10;

    public int CurrentPage { get; set; } = 1;

    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public void NextPage()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
        }
    }

    public void PreviousPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
        }
    }
}