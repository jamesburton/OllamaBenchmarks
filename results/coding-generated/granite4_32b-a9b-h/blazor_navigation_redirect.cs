using Microsoft.AspNetCore.Components;

[Inject]
public NavigationManager Nav { get; set; } = null!;

[Inject]
public IProductService ProductService { get; set; } = null!;

public string? ErrorMessage { get; set; }

public async Task DeleteProduct()
{
    bool success = await ProductService.DeleteAsync(ProductId);
    if (success)
    {
        Nav.NavigateTo("/products");
    }
    else
    {
        ErrorMessage = "Delete failed";
    }
}