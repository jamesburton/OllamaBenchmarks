using Microsoft.EntityFrameworkCore;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    public DbSet<TodoItem> TodoItems { get; set; } = null!;
}

public interface ITodoService
{
    Task<TodoItem> CreateAsync(string title);
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService
{
    private readonly TodoDbContext _db;
    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem> CreateAsync(string title)
    {
        var item = new TodoItem
        {
            Title = title,
            IsComplete = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.TodoItems.Add(item);
        await _db.SaveChangesAsync();
        return item;
    }

    public Task<List<TodoItem>> GetAllAsync() =>
        _db.TodoItems.ToListAsync();

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;

        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;

        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

// -------------------------------------------------
// Example tests (using global usings for Xunit & AwesomeAssertions)
// -------------------------------------------------
public class TodoServiceTests
{
    private TodoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TodoDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_AddsItem()
    {
        using var ctx = CreateContext();
        var service = new TodoService(ctx);

        var item = await service.CreateAsync("Test");

        item.Id.Should().BeGreaterThan(0);
        item.Title.Should().Be("Test");
        item.IsComplete.Should().BeFalse();
        item.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CompleteAsync_UpdatesItem()
    {
        using var ctx = CreateContext();
        var service = new TodoService(ctx);
        var item = await service.CreateAsync("Complete me");

        var result = await service.CompleteAsync(item.Id);
        result.Should().BeTrue();

        var fetched = await service.GetByIdAsync(item.Id);
        fetched!.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        using var ctx = CreateContext();
        var service = new TodoService(ctx);
        var item = await service.CreateAsync("Delete me");

        var deleted = await service.DeleteAsync(item.Id);
        deleted.Should().BeTrue();

        var fetched = await service.GetByIdAsync(item.Id);
        fetched.Should().BeNull();
    }
}