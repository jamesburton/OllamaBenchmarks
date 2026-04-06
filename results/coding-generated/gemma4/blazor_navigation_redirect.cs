using Microsoft.AspNetCore.Components;

// 1. IProductService interface
public interface IProductService
{
    Task<bool> DeleteAsync(int id);
}

// 2. ProductDetailBase class
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
        // Reset error message before attempting delete
        ErrorMessage = null;

        if (ProductService == null)
        {
            ErrorMessage = "Service not injected.";
            return;
        }

        bool success = await ProductService.DeleteAsync(ProductId);

        if (success)
        {
            // Success: Navigate to the product list
            Nav.NavigateTo("/products");
        }
        else
        {
            // Failure: Set error message
            ErrorMessage = "Delete failed";
        }
    }
}