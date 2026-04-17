using Microsoft.AspNetCore.Mvc;

public record CreateProductRequest(string Name, string Category, decimal Price, int Stock);
public record ProductDto(int Id, string Name, string Category, decimal Price, int Stock);

public interface IProductService
{
    Task<ProductDto> CreateAsync(CreateProductRequest req, CancellationToken ct);
    Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}

public class InMemoryProductService : IProductService
{
    private readonly List<ProductDto> _products = new();
    private int _nextId = 1;
    private readonly object _lock = new();

    public Task<ProductDto> CreateAsync(CreateProductRequest req, CancellationToken ct)
    {
        ProductDto product;
        lock (_lock)
        {
            product = new ProductDto(_nextId++, req.Name, req.Category, req.Price, req.Stock);
            _products.Add(product);
        }
        return Task.FromResult(product);
    }

    public Task<ProductDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        ProductDto? product;
        lock (_lock)
        {
            product = _products.FirstOrDefault(p => p.Id == id);
        }
        return Task.FromResult(product);
    }

    public Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken ct)
    {
        IReadOnlyList<ProductDto> products;
        lock (_lock)
        {
            products = _products.ToList().AsReadOnly();
        }
        return Task.FromResult(products);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        bool removed;
        lock (_lock)
        {
            removed = _products.RemoveAll(p => p.Id == id) > 0;
        }
        return Task.FromResult(removed);
    }
}

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/products");

        group.MapPost("/", async (CreateProductRequest req, IProductService service, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.BadRequest("Name cannot be empty.");
            if (req.Price <= 0)
                return Results.BadRequest("Price must be greater than 0.");

            var product = await service.CreateAsync(req, ct);
            return Results.Created($"/products/{product.Id}", product);
        });

        group.MapGet("/{id}", async (int id, IProductService service, CancellationToken ct) =>
        {
            var product = await service.GetByIdAsync(id, ct);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        });

        group.MapGet("/", async (IProductService service, CancellationToken ct) =>
        {
            var products = await service.GetAllAsync(ct);
            return Results.Ok(products);
        });

        group.MapDelete("/{id}", async (int id, IProductService service, CancellationToken ct) =>
        {
            var deleted = await service.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });
    }
}