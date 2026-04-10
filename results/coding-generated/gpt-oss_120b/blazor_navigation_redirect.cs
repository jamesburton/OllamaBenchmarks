using Microsoft.AspNetCore.Components;

public interface IProductService
{
    System.Threading.Tasks.Task<bool> DeleteAsync(int id);
}

public class ProductDetailBase : ComponentBase
{
    [Parameter]
    public int ProductId { get; set; }

    [Inject]
    public NavigationManager Nav { get; set; } = null!;

    [Inject]
    public IProductService ProductService { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public async System.Threading.Tasks.Task DeleteProduct()
    {
        var success = await ProductService.DeleteAsync(ProductId);
        if (success)
        {
            Nav.NavigateTo("/products");
        }
        else
        {
            ErrorMessage = "Delete failed";
        }
    }
}