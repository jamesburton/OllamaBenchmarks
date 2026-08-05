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
        if (await ProductService.DeleteAsync(ProductId))
        {
            Nav.NavigateTo("/products");
        }
        else
        {
            ErrorMessage = "Delete failed";
        }
    }
}