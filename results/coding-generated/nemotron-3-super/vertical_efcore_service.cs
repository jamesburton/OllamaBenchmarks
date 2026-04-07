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
    public DbSet<TodoItem> TodoItems { get; set; } = default!;
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

    public async Task<List<TodoItem>> GetAllAsync() =>
        await _db.TodoItems.ToListAsync();

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

public class TodoServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsAndReturnsItem()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
            .Options;
        await using var db = new TodoDbContext(options);
        var service = new TodoService(db);

        var result = await service.CreateAsync("Test Todo");

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Todo");
        result.IsComplete.Should().BeFalse();
        result.CreatedAt.Should().BeCloseTo(System.DateTime.UtcNow, precision: System.TimeSpan.FromSeconds(2));

        var dbItem = await db.TodoItems.FindAsync(result.Id);
        dbItem.Should().NotBeNull();
        dbItem.Title.Should().Be("Test Todo");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllItems()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
            .Options;
        await using var db = new TodoDbContext(options);
        db.TodoItems.Add(new TodoItem { Title = "One", IsComplete = false, CreatedAt = System.DateTime.UtcNow });
        db.TodoItems.Add(new TodoItem { Title = "Two", IsComplete = true, CreatedAt = System.DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new TodoService(db);
        var items = await service.GetAllAsync();

        items.Should().HaveCount(2);
        items.Should().ContainSingle(i => i.Title == "One");
        items.Should().ContainSingle(i => i.Title == "Two");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsItemOrNull()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
            .Options;
        await using var db = new TodoDbContext(options);
        var item = new TodoItem { Title = "FindMe", IsComplete = false, CreatedAt = System.DateTime.UtcNow };
        db.TodoItems.Add(item);
        await db.SaveChangesAsync();

        var service = new TodoService(db);

        var found = await service.GetByIdAsync(item.Id);
        found.Should().NotBeNull();
        found.Title.Should().Be("FindMe");

        var notFound = await service.GetByIdAsync(999);
        notFound.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_MarksItemComplete()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
            .Options;
        await using var db = new TodoDbContext(options);
        var item = new TodoItem { Title = "ToComplete", IsComplete = false, CreatedAt = System.DateTime.UtcNow };
        db.TodoItems.Add(item);
        await db.SaveChangesAsync();

        var service = new TodoService(db);

        var result = await service.CompleteAsync(item.Id);
        result.Should().BeTrue();

        var updated = await db.TodoItems.FindAsync(item.Id);
        updated.IsComplete.Should().BeTrue();

        var falseResult = await service.CompleteAsync(999);
        falseResult.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
            .Options;
        await using var db = new TodoDbContext(options);
        var item = new TodoItem { Title = "ToDelete", IsComplete = false, CreatedAt = System.DateTime.UtcNow };
        db.TodoItems.Add(item);
        await db.SaveChangesAsync();

        var service = new TodoService(db);

        var result = await service.DeleteAsync(item.Id);
        result.Should().BeTrue();

        var deleted = await db.TodoItems.FindAsync(item.Id);
        deleted.Should().BeNull();

        var falseResult = await service.DeleteAsync(999);
        falseResult.Should().BeFalse();
    }
}