using Microsoft.EntityFrameworkCore;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
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
        {
            return false;
        }

        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null)
        {
            return false;
        }

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
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TodoDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_WithValidTitle_PersistsItem()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new TodoService(db);

        // Act
        var result = await service.CreateAsync("Buy milk");

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Buy milk");
        result.IsComplete.Should().BeFalse();
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
        result.Id.Should().BeGreaterThan(0);

        var stored = await service.GetByIdAsync(result.Id);
        stored.Should().NotBeNull();
        stored!.Title.Should().Be("Buy milk");
    }

    [Fact]
    public async Task GetAllAsync_WithNoItems_ReturnsEmptyList()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new TodoService(db);

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleItems_ReturnsAll()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new TodoService(db);
        await service.CreateAsync("Task A");
        await service.CreateAsync("Task B");
        await service.CreateAsync("Task C");

        // Act
        var result = await service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(t => t.Title).Should().BeEquivalentTo(new[] { "Task A", "Task B", "Task C" });
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ReturnsItem()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new TodoService(db);
        var created = await service.CreateAsync("Find me");

        // Act
        var result = await service.GetByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(created.Id);
        result.Title.Should().Be("Find me");
        result.IsComplete.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WithMissingId_ReturnsNull()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new TodoService(db);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CompleteAsync_WithExistingId_SetsIsCompleteTrue()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new TodoService(db);
        var created = await service.CreateAsync("Finish me");

        // Act
        var ok = await service.CompleteAsync(created.Id);

        // Assert
        ok.Should().BeTrue();
        var fetched = await service.GetByIdAsync(created.Id);
        fetched.Should().NotBeNull();
        fetched!.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteAsync_WithMissingId_ReturnsFalse()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new TodoService(db);

        // Act
        var ok = await service.CompleteAsync(42);

        // Assert
        ok.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_RemovesItem()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new TodoService(db);
        var created = await service.CreateAsync("Remove me");

        // Act
        var ok = await service.DeleteAsync(created.Id);

        // Assert
        ok.Should().BeTrue();
        (await service.GetAllAsync()).Should().BeEmpty();
        (await service.GetByIdAsync(created.Id)).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithMissingId_ReturnsFalse()
    {
        // Arrange
        using var db = CreateDbContext();
        var service = new TodoService(db);

        // Act
        var ok = await service.DeleteAsync(404);

        // Assert
        ok.Should().BeFalse();
    }
}