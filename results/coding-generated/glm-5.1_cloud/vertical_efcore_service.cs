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
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

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
    private static TodoDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TodoDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_SetsDefaultsAndPersists()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.CreateAsync("Write tests");

        result.Should().NotBeNull();
        result.Title.Should().Be("Write tests");
        result.IsComplete.Should().BeFalse();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Id.Should().BeGreaterThan(0);

        var all = await service.GetAllAsync();
        all.Should().ContainSingle(x => x.Id == result.Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllItems()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db);
        await service.CreateAsync("A");
        await service.CreateAsync("B");
        await service.CreateAsync("C");

        var result = await service.GetAllAsync();

        result.Should().HaveCount(3);
        result.Select(x => x.Title).Should().BeEquivalentTo("A", "B", "C");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsItem_WhenFound()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db);
        var created = await service.CreateAsync("Find me");

        var result = await service.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Find me");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_ReturnsTrueAndMarksComplete_WhenFound()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db);
        var created = await service.CreateAsync("Pending");

        var result = await service.CompleteAsync(created.Id);

        result.Should().BeTrue();
        var updated = await service.GetByIdAsync(created.Id);
        updated!.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteAsync_ReturnsFalse_WhenNotFound()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.CompleteAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrueAndRemovesItem_WhenFound()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db);
        var created = await service.CreateAsync("Delete me");

        var result = await service.DeleteAsync(created.Id);

        result.Should().BeTrue();
        var gone = await service.GetByIdAsync(created.Id);
        gone.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        using var db = CreateDbContext();
        var service = new TodoService(db);

        var result = await service.DeleteAsync(999);

        result.Should().BeFalse();
    }
}