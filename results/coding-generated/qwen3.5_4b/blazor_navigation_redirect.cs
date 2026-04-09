public interface IProductService
        {
            Task<bool> DeleteAsync(int id);
        }

public class ProductDetailBase : Microsoft.AspNetCore.Components.ComponentBase
        {
            [Parameter] public int ProductId { get; set; }

            [Inject] public NavigationManager Nav { get; set; } = null!;
            [Inject] public IProductService ProductService { get; set; } = null!;

            public string? ErrorMessage { get; set; }

            public async Task DeleteProduct()
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

using Microsoft.AspNetCore.Components;

public interface IProductService
{
    Task<bool> DeleteAsync(int id);
}

public class ProductDetailBase : Microsoft.AspNetCore.Components.ComponentBase
{
    [Parameter] public int ProductId { get; set; }

    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public IProductService ProductService { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public async Task DeleteProduct()
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

razor
@inject NavigationManager Nav

var success = await ProductService.DeleteAsync(ProductId);
if (success)
{
    Nav.NavigateTo("/products");
}
else
{
    ErrorMessage = "Delete failed";
}

using Microsoft.AspNetCore.Components;

public interface IProductService
{
    Task<bool> DeleteAsync(int id);
}

public class ProductDetailBase : Microsoft.AspNetCore.Components.ComponentBase
{
    [Parameter] public int ProductId { get; set; }

    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public IProductService ProductService { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public async Task DeleteProduct()
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

using Microsoft.AspNetCore.Components;

public interface IProductService
{
    Task<bool> DeleteAsync(int id);
}

public class ProductDetailBase : ComponentBase
{
    [Parameter] public int ProductId { get; set; }

    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public IProductService ProductService { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public async Task DeleteProduct()
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

using Microsoft.AspNetCore.Components;

public interface IProductService
{
    Task<bool> DeleteAsync(int id);
}

public class ProductDetailBase : ComponentBase
{
    [Parameter] public int ProductId { get; set; }

    [Inject] public NavigationManager Nav { get; set; } = null!;
    [Inject] public IProductService ProductService { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public async Task DeleteProduct()
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