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

razor
@inject IOrderService Orders
@inject NavigationManager Nav

[Parameter] public string Title { get; set; } = "";

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