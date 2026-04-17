using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddSingleton<ICatalogRepository, InMemoryCatalogRepository>();

var app = builder.Build();

// Endpoints
app.MapCatalogEndpoints();

app.Run();

// --- Domain ---
public record CatalogItem(int Id, string Name, string Category, decimal Price, DateTime LastModified);

// --- Repository Interface ---
public interface ICatalogRepository
{
    Task<IReadOnlyList<CatalogItem>> GetAllAsync(CancellationToken ct);
    Task<CatalogItem?> GetByIdAsync(int id, CancellationToken ct);
    Task UpdateAsync(CatalogItem item, CancellationToken ct);
}

// --- Repository Implementation ---
public class InMemoryCatalogRepository : ICatalogRepository
{
    private readonly List<CatalogItem> _items = new()
    {
        new CatalogItem(1, "Laptop", "Electronics", 999.99m, DateTime.UtcNow),
        new CatalogItem(2, "Mouse", "Electronics", 25.50m, DateTime.UtcNow),
        new CatalogItem(3, "Desk Chair", "Furniture", 150.00m, DateTime.UtcNow)
    };

    public Task<IReadOnlyList<CatalogItem>> GetAllAsync(CancellationToken ct) 
        => Task.FromResult<IReadOnlyList<CatalogItem>>(_items.AsReadOnly());

    public Task<CatalogItem?> GetByIdAsync(int id, CancellationToken ct) 
        => Task.FromResult(_items.FirstOrDefault(i => i.Id == id));

    public Task UpdateAsync(CatalogItem item, CancellationToken ct)
    {
        var index = _items.FindIndex(i => i.Id == item.Id);
        if (index >= 0)
        {
            _items[index] = item with { LastModified = DateTime.UtcNow };
        }
        return Task.CompletedTask;
    }
}

// --- Endpoints ---
public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this WebApplication app)
    {
        var group = app.MapGet("/catalog", GetAllItems);
        group = app.MapGet("/catalog/{id}", GetItemById);
        group = app.MapPut("/catalog/{id}", UpdateItem);
    }

    private static async Task<IResult> GetAllItems(ICatalogRepository repo, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);
        var json = JsonSerializer.Serialize(items);
        var etag = ETagHelper.ComputeETag(json);

        return Results.Ok(new { content = json, etag });
    }

    // Note: Minimal API automatically handles JSON serialization. 
    // To inject headers manually, we use an extension or wrapper, 
    // but for this single file slice, we return a custom result or use the built-in mechanism.
    // Here is a cleaner implementation for the requirements:

    private static async Task<IResult> GetAllItems_Impl(HttpContext httpContext, ICatalogRepository repo, CancellationToken ct)
    {
        var items = await repo.GetAllAsync(ct);
        // Serialize to compute ETag based on content
        var json = JsonSerializer.Serialize(items);
        var etag = ETagHelper.ComputeETag(json);

        httpContext.Response.Headers.ETag = etag;
        return Results.Ok(items);
    }

    private static async Task<IResult> GetItemById(int id, HttpContext httpContext, ICatalogRepository repo, CancellationToken ct)
    {
        var item = await repo.GetByIdAsync(id, ct);
        if (item is null) return Results.NotFound();

        var etag = ETagHelper.ComputeETag(item);

        // Check If-None-Match for 304 Not Modified
        if (httpContext.Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && 
            ETagHelper.IsMatch(etag, ifNoneMatch.ToString()))
        {
            return Results.StatusCode(StatusCodes.Status304NotModified);
        }

        httpContext.Response.Headers.ETag = etag;
        return Results.Ok(item);
    }

    private static async Task<IResult> UpdateItem(int id, CatalogItemUpdateModel update, HttpContext httpContext, ICatalogRepository repo, CancellationToken ct)
    {
        // Validate If-Match header
        if (!httpContext.Request.Headers.TryGetValue("If-Match", out var ifMatch) || string.IsNullOrEmpty(ifMatch))
        {
            return Results.BadRequest("If-Match header is required.");
        }

        var existingItem = await repo.GetByIdAsync(id, ct);
        if (existingItem is null) return Results.NotFound();

        var currentEtag = ETagHelper.ComputeETag(existingItem);

        // Check concurrency
        if (!ETagHelper.IsMatch(currentEtag, ifMatch.ToString()))
        {
            return Results.StatusCode(StatusCodes.Status412PreconditionFailed);
        }

        var updatedItem = existingItem with 
        { 
            Name = update.Name, 
            Category = update.Category, 
            Price = update.Price 
        };

        await repo.UpdateAsync(updatedItem, ct);

        // Return updated item with new ETag
        var newEtag = ETagHelper.ComputeETag(updatedItem);
        httpContext.Response.Headers.ETag = newEtag;
        return Results.Ok(updatedItem);
    }

    // Helper record for input
    public record CatalogItemUpdateModel(string Name, string Category, decimal Price);
}

// --- ETag Helper ---
public static class ETagHelper
{
    public static string ComputeETag(object obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return ComputeETag(json);
    }

    public static string ComputeETag(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = MD5.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool IsMatch(string etag, string ifNoneMatch)
    {
        if (string.IsNullOrEmpty(ifNoneMatch)) return false;

        // Handle quoted ETags (standard format: "etag")
        var clientEtag = ifNoneMatch.Trim('"');
        return string.Equals(etag, clientEtag, StringComparison.Ordinal);
    }
}

// Extension to hook up the implementation cleanly
public static class EndpointExtensions
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/catalog", CatalogEndpoints.GetAllItems_Impl);
        endpoints.MapGet("/catalog/{id}", CatalogEndpoints.GetItemById);
        endpoints.MapPut("/catalog/{id}", CatalogEndpoints.UpdateItem);
    }
}