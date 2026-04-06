using Microsoft.AspNetCore.Components;

public class ProductDetailBase
{
    public int ProductId { get; set; }
    [Inject] public NavigationManager Nav { get; set; }
    [Inject] public IProductService ProductService { get; set; }
    public string? ErrorMessage { get; set; }
    public async Task DeleteProduct() => await ProductService.DeleteAsync(ProductId);
}