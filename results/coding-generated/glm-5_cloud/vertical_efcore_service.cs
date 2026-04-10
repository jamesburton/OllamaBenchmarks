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
        if (item == null)
            return false;

        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null)
            return false;

        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

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
    public async Task CreateAsync_ShouldCreateTodoItemWithCorrectProperties()
    {
        using var db = CreateContext();
        var service = new TodoService(db);

        var result = await service.CreateAsync("Test Todo");

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Todo");
        result.IsComplete.Should().BeFalse();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllTodoItems()
    {
        using var db = CreateContext();
        var service = new TodoService(db);
        await service.CreateAsync("Todo 1");
        await service.CreateAsync("Todo 2");

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Title == "Todo 1");
        result.Should().Contain(x => x.Title == "Todo 2");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnItem_WhenExists()
    {
        using var db = CreateContext();
        var service = new TodoService(db);
        var created = await service.CreateAsync("Test");

        var result = await service.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        using var db = CreateContext();
        var service = new TodoService(db);

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_ShouldReturnTrueAndMarkComplete_WhenItemExists()
    {
        using var db = CreateContext();
        var service = new TodoService(db);
        var created = await service.CreateAsync("Test");

        var result = await service.CompleteAsync(created.Id);

        result.Should().BeTrue();
        var item = await service.GetByIdAsync(created.Id);
        item!.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteAsync_ShouldReturnFalse_WhenItemNotExists()
    {
        using var db = CreateContext();
        var service = new TodoService(db);

        var result = await service.CompleteAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnTrueAndRemoveItem_WhenItemExists()
    {
        using var db = CreateContext();
        var service = new TodoService(db);
        var created = await service.CreateAsync("Test");

        var result = await service.DeleteAsync(created.Id);

        result.Should().BeTrue();
        var item = await service.GetByIdAsync(created.Id);
        item.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenItemNotExists()
    {
        using var db = CreateContext();
        var service = new TodoService(db);

        var result = await service.DeleteAsync(999);

        result.Should().BeFalse();
    }
}