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
    public TodoService(TodoDbContext db) => _db = db;
    public Task<TodoItem> CreateAsync(string title)
    {
        var item = new TodoItem { Title = title, IsComplete = false, CreatedAt = DateTime.UtcNow };
        _db.TodoItems.Add(item);
        await _db.SaveChangesAsync();
        return Task.FromResult(item);
    }
    public Task<List<TodoItem>> GetAllAsync() => _db.TodoItems.ToListAsync();
    public Task<TodoItem?> GetByIdAsync(int id) => _db.TodoItems.FindAsync(id);
    public Task<bool> CompleteAsync(int id)
    {
        var item = _db.TodoItems.FindAsync(id);
        if (item == null) return Task.FromResult(false);
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return Task.FromResult(true);
    }
    public Task<bool> DeleteAsync(int id)
    {
        var item = _db.TodoItems.FindAsync(id);
        if (item == null) return Task.FromResult(false);
        _db.TodoItems.Remove(item);
        return Task.FromResult(await _db.SaveChangesAsync() > 0);
    }
}