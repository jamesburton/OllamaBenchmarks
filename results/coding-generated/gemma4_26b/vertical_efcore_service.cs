using Microsoft.EntityFrameworkCore;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }
    public DbSet<TodoItem> TodoItems { get; set; }
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

    public TodoService(TodoDbContext db)
    {
        _db = db;
    }

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

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await _db.TodoItems.ToListAsync();
    }

    public async Task<TodoItem?> GetByIdAsync(int id)
    {
        return await _db.TodoItems.FindAsync(id);
    }

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

public class TodoServiceTests
{
    private DbContextOptions<TodoDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Xunit.Fact]
    public async Task CreateAsync_ShouldSaveItem()
    {
        var options = CreateOptions();
        using var db = new TodoDbContext(options);
        var service = new TodoService(db);

        var result = await service.CreateAsync("Test Task");

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Task");
        result.IsComplete.Should().BeFalse();

        var savedItem = await db.TodoItems.FindAsync(result.Id);
        savedItem.Should().NotBeNull();
    }

    [Xunit.Fact]
    public async Task CompleteAsync_ShouldUpdateStatus()
    {
        var options = CreateOptions();
        using var db = new TodoDbContext(options);
        var service = new TodoService(db);
        var item = await service.CreateAsync("Pending Task");

        var success = await service.CompleteAsync(item.Id);

        success.Should().BeTrue();
        item.IsComplete.Should().BeTrue();
    }

    [Xunit.Fact]
    public async Task DeleteAsync_ShouldRemoveItem()
    {
        var options = CreateOptions();
        using var db = new TodoDbContext(options);
        var service = new TodoService(db);
        var item = await service.CreateAsync("To Delete");

        var success = await service.DeleteAsync(item.Id);

        success.Should().BeTrue();
        var deletedItem = await db.TodoItems.FindAsync(item.Id);
        deletedItem.Should().BeNull();
    }

    [Xunit.Fact]
    public async Task GetByIdAsync_ShouldReturnNullIfNotFound()
    {
        var options = CreateOptions();
        using var db = new TodoDbContext(options);
        var service = new TodoService(db);

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }
}