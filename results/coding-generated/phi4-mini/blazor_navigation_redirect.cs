using Microsoft.AspNetCore.Components;

[Parameter] public int ProductId { get; set; }
[Inject] public NavigationManager Nav { get; set; } = null!;
[Inject] public IProductService ProductService { get; set; } = null!;

// Constructor is not needed as we are using default parameters

public string? ErrorMessage { get; private set; }

protected override void OnInitialized()
{
    base.OnInitialized();
}

private async Task DeleteProduct()
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