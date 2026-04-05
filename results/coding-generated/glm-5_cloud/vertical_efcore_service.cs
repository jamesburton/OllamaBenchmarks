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
    private TodoDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TodoDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateTodoItem()
    {
        await using var db = CreateContext("Create_Test");
        var service = new TodoService(db);

        var result = await service.CreateAsync("Test Item");

        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("Test Item");
        result.IsComplete.Should().BeFalse();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllItems()
    {
        await using var db = CreateContext("GetAll_Test");
        var service = new TodoService(db);
        await service.CreateAsync("Item 1");
        await service.CreateAsync("Item 2");

        var result = await service.GetAllAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Title == "Item 1");
        result.Should().Contain(x => x.Title == "Item 2");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnItem_WhenExists()
    {
        await using var db = CreateContext("GetById_Exists_Test");
        var service = new TodoService(db);
        var created = await service.CreateAsync("Find Me");

        var result = await service.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
    {
        await using var db = CreateContext("GetById_NotExists_Test");
        var service = new TodoService(db);

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_ShouldSetIsComplete_AndReturnTrue()
    {
        await using var db = CreateContext("Complete_Test");
        var service = new TodoService(db);
        var created = await service.CreateAsync("Complete Me");

        var result = await service.CompleteAsync(created.Id);
        var item = await db.TodoItems.FindAsync(created.Id);

        result.Should().BeTrue();
        item!.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        await using var db = CreateContext("Complete_NotFound_Test");
        var service = new TodoService(db);

        var result = await service.CompleteAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveItem_AndReturnTrue()
    {
        await using var db = CreateContext("Delete_Test");
        var service = new TodoService(db);
        var created = await service.CreateAsync("Delete Me");

        var result = await service.DeleteAsync(created.Id);
        var item = await db.TodoItems.FindAsync(created.Id);

        result.Should().BeTrue();
        item.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenNotFound()
    {
        await using var db = CreateContext("Delete_NotFound_Test");
        var service = new TodoService(db);

        var result = await service.DeleteAsync(999);

        result.Should().BeFalse();
    }
}