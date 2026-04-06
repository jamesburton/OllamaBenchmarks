using Microsoft.EntityFrameworkCore;
using AwesomeAssertions;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TodoDbContext : DbContext {
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems { get; set; }
}

public interface ITodoService {
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
        var item = new TodoItem { Title = title, IsComplete = false, CreatedAt = DateTime.UtcNow };
        await _db.TodoItems.AddAsync(item);
        return await _db.SaveChangesAsync();
    }

    public async Task<List<TodoItem>> GetAllAsync() => await _db.TodoItems.ToListAsync();

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        (await _db.TodoItems.FindAsync(id)) ?? null;

    public async Task<bool> CompleteAsync(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (!item) return false;
        item.IsComplete = true;
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id) =>
        (await _db.TodoItems.RemoveAsync(item)) ?? false;
}