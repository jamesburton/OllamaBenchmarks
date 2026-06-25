using Microsoft.AspNetCore.Components;

namespace BlazorComponentExample
{
    public partial class ProductDetailBase : ComponentBase
    {
        [Parameter] public int ProductId { get; set; }
        [Inject] public NavigationManager Nav { get; set; } = null;
        [Inject] public IProductService ProductService { get; set; } = null;

        private string? _ErrorMessage;

        protected override async Task OnInitializedAsync()
        {
            if (ProductId > 0)
                await ProductService.GetProductByIdAsync(ProductId);
        }

        public async Task DeleteProduct()
        {
            if (_ErrorMessage != null) return;
            try
            {
                await ProductService.DeleteAsync(ProductId);
                Nav.NavigateTo("/products");
            }
            catch (Exception ex)
            {
                _ErrorMessage = $"Delete failed: {ex.Message}";
            }
        }
    }

    public partial class Counter : ComponentBase
    {
        [Parameter] public string Title { get; set; } = "";
        [Parameter] public EventCallback<string> OnChange { get; set; }
        [CascadingParameter] public ThemeState Theme { get; set; } = default;

        async Task HandleClick()
        {
            await OnChange.InvokeAsync("new value");
        }

    }

}