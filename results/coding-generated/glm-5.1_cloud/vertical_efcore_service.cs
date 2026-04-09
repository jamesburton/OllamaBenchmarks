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
    private static TodoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TodoDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_SetsDefaultsAndSaves()
    {
        var db = CreateContext();
        var service = new TodoService(db);

        var result = await service.CreateAsync("Test item");

        result.Should().NotBeNull();
        result.Title.Should().Be("Test item");
        result.IsComplete.Should().BeFalse();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Id.Should().BeGreaterThan(0);

        var all = await service.GetAllAsync();
        all.Should().HaveCount(1);
        all[0].Should().BeEquivalentTo(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllItems()
    {
        var db = CreateContext();
        var service = new TodoService(db);

        await service.CreateAsync("First");
        await service.CreateAsync("Second");
        await service.CreateAsync("Third");

        var result = await service.GetAllAsync();

        result.Should().HaveCount(3);
        result.Should().Contain(x => x.Title == "First");
        result.Should().Contain(x => x.Title == "Second");
        result.Should().Contain(x => x.Title == "Third");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsItem_WhenExists()
    {
        var db = CreateContext();
        var service = new TodoService(db);

        var created = await service.CreateAsync("Find me");

        var result = await service.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var db = CreateContext();
        var service = new TodoService(db);

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_SetsIsComplete_AndReturnsTrue()
    {
        var db = CreateContext();
        var service = new TodoService(db);

        var created = await service.CreateAsync("To complete");

        var result = await service.CompleteAsync(created.Id);

        result.Should().BeTrue();
        var updated = await service.GetByIdAsync(created.Id);
        updated!.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteAsync_ReturnsFalse_WhenNotFound()
    {
        var db = CreateContext();
        var service = new TodoService(db);

        var result = await service.CompleteAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_RemovesItem_AndReturnsTrue()
    {
        var db = CreateContext();
        var service = new TodoService(db);

        var created = await service.CreateAsync("To delete");

        var result = await service.DeleteAsync(created.Id);

        result.Should().BeTrue();
        var all = await service.GetAllAsync();
        all.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        var db = CreateContext();
        var service = new TodoService(db);

        var result = await service.DeleteAsync(999);

        result.Should().BeFalse();
    }
}