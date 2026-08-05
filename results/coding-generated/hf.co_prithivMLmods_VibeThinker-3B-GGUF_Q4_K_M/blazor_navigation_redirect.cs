using Microsoft.AspNetCore_components;

IProductService DeleteAsync(int id);
public class ProductService : IProductService
{
    public Task<bool> DeleteAsync(int id) 
        => Task.CompletedTask;
}

public class ProductDetailBase : ComponentBase
{
    [Parameter] public int ProductId { get; set; }
    [Inject] public NavigationManager Nav { get; set; } = null!
    [Inject] public IProductServiceProductService { get; set; } = null!
    public string? errorMessage { get; set; }

    public async Task DeleteProduct()
    {
        if (Nav == null || productService == null)
            return;

        var success = await productService.DeleteAsync(ProductId);
        if (success)
        {
            Nav.NavigateTo("/products");
        }
        else
        {
            errorMessage = "Delete failed";
        }
    }
}