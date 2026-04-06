using Microsoft.AspNetCore.Components;

// IProductService interface
public interface IProductService
{
    Task<bool> DeleteAsync(int id);
}

// ProductDetailBase class
public class ProductDetailBase : ComponentBase
{
    [Parameter]
    public int ProductId { get; set; }

    [Inject]
    public NavigationManager Nav { get; set; } = null!;

    [Inject]
    public IProductService ProductService { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public async Task DeleteProduct()
    {
        var result = await ProductService.DeleteAsync(ProductId);
        if (result)
        {
            Nav.NavigateTo("/products");
        }
        else
        {
            ErrorMessage = "Delete failed";
        }
    }
}