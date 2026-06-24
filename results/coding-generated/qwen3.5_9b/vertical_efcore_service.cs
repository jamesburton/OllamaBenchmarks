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
    public TodoDbContext(DbContextOptions<TodoDbContext> options) 
        : base(options) {}

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
        => _db = db;

    public async Task<TodoItem> CreateAsync(string title)
    {
        var item = new TodoItem 
        { 
            Title = title, 
            IsComplete = false, 
            CreatedAt = DateTime.UtcNow 
        };

        await _db.TodoItems.AddAsync(item); // Or Add then SaveChanges. Using async add is safer for consistency but prompt implies standard flow. Let's use synchronous Add to match "adds it... calls SaveChanges". Actually EF Core 10 prefers Async everywhere usually, but I will stick to the logic: Add -> SaveChanges.
        await _db.SaveChangesAsync(); // Wait, if using AddAsync, we don't need sync add then async save? 
        // Let's use standard pattern for clarity and compliance with "calls SaveChangesAsync".

        var newItem = new TodoItem { Title = title, IsComplete = false, CreatedAt = DateTime.UtcNow };
        _db.TodoItems.Add(newItem);
        await _db.SaveChangesAsync();
        return newItem;
    }

    public async Task<List<TodoItem>> GetAllAsync() => await _db.TodoItems.ToListAsync();

    public async Task<TodoItem?> GetByIdAsync(int id) 
        => await _db.TodoItems.FindAsync(id);

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

        _db.TodoItems.Remove(item); // Or RemoveRange? No, single. 
        await _db.SaveChangesAsync();
        return true;
    }
}