using Microsoft.AspNetCore Components;

public interface IProdService
{
    Task<bool> DeleteAsync(int id);
}

public class Product DetailBase : Microsoft.AspNetCore components.ComponentBase
{
    [Parameter] public int ProductId { get; set; }
    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public IProdService ProductService { get; set; } = null!;
    public string? ErrorMessage { get; set; }

    public async Task DeleteProduct()
    {
        var result = await _productService.DeleteAsync(ProductId);

        if (result)
        {
            Nav.NavigateTo("/products");
        }
        else
        {
            MessageText = "Delete failed";
        }
    }
}