using Microsoft.AspNetCore.Mvc;

public static class FileEndpoints
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public static WebApplication MapFileEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/files");

        group.MapPost("/", async (IFormFile file, IFileStorage storage, CancellationToken ct) =>
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest("File is missing or empty.");
            }

            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Invalid content type. Only images are allowed.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return Results.BadRequest($"File size exceeds the limit of {MaxFileSizeBytes / (1024 * 1024)}MB.");
            }

            await using var stream = file.OpenReadStream();
            var metadata = await storage.StoreAsync(file.FileName, file.ContentType, stream, ct);

            return Results.Created($"/files/{metadata.Id}", metadata);
        })
        .DisableAntiforgery(); // Required for minimal APIs handling multipart/form-data

        group.MapGet("/{id:guid}", async (Guid id, IFileStorage storage, CancellationToken ct) =>
        {
            var result = await storage.GetAsync(id, ct);

            if (result is null)
            {
                return Results.NotFound();
            }

            var (meta, content) = result.Value;
            return Results.Stream(content, meta.ContentType, meta.FileName);
        });

        group.MapDelete("/{id:guid}", async (Guid id, IFileStorage storage, CancellationToken ct) =>
        {
            var deleted = await storage.DeleteAsync(id, ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        });

        group.MapGet("/", async (IFileStorage storage, CancellationToken ct) =>
        {
            var files = await storage.ListAsync(ct);
            return Results.Ok(files);
        });

        return app;
    }
}

public record FileMetadata(Guid Id, string FileName, string ContentType, long SizeBytes, DateTime UploadedAt);

public interface IFileStorage
{
    Task<FileMetadata> StoreAsync(string fileName, string contentType, Stream content, CancellationToken ct);
    Task<(FileMetadata Meta, Stream Content)?> GetAsync(Guid id, CancellationToken ct);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<FileMetadata>> ListAsync(CancellationToken ct);
}

public class InMemoryFileStorage : IFileStorage
{
    private readonly Dictionary<Guid, (FileMetadata Meta, byte[] Content)> _store = new();

    public Task<FileMetadata> StoreAsync(string fileName, string contentType, Stream content, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        var meta = new FileMetadata(id, fileName, contentType, content.Length, DateTime.UtcNow);

        using var memoryStream = new MemoryStream();
        content.CopyTo(memoryStream);
        var data = memoryStream.ToArray();

        _store[id] = (meta, data);

        return Task.FromResult(meta);
    }

    public Task<(FileMetadata Meta, Stream Content)?> GetAsync(Guid id, CancellationToken ct)
    {
        if (_store.TryGetValue(id, out var entry))
        {
            Stream contentStream = new MemoryStream(entry.Content);
            return Task.FromResult<(FileMetadata Meta, Stream Content)?>((entry.Meta, contentStream));
        }

        return Task.FromResult<(FileMetadata Meta, Stream Content)?>(null);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        return Task.FromResult(_store.Remove(id));
    }

    public Task<IReadOnlyList<FileMetadata>> ListAsync(CancellationToken ct)
    {
        var list = _store.Values.Select(v => v.Meta).ToList().AsReadOnly();
        return Task.FromResult<IReadOnlyList<FileMetadata>>(list);
    }
}