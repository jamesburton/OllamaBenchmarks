using Microsoft.EntityFrameworkCore;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public System.DateTime CreatedAt { get; set; }
}

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    public DbSet<TodoItem> TodoItems { get; set; } = null!;
}

public interface ITodoService
{
    System.Threading.Tasks.Task<TodoItem> CreateAsync(string title);
    System.Threading.Tasks.Task<System.Collections.Generic.List<TodoItem>> GetAllAsync();
    System.Threading.Tasks.Task<TodoItem?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<bool> CompleteAsync(int id);
    System.Threading.Tasks.Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService
{
    private readonly TodoDbContext db;

    public TodoService(TodoDbContext db)
    {
        this.db = db;
    }

    public async System.Threading.Tasks.Task<TodoItem> CreateAsync(string title)
    {
        var item = new TodoItem
        {
            Title = title,
            IsComplete = false,
            CreatedAt = System.DateTime.UtcNow
        };
        db.TodoItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async System.Threading.Tasks.Task<System.Collections.Generic.List<TodoItem>> GetAllAsync()
    {
        return await db.TodoItems.ToListAsync();
    }

    public async System.Threading.Tasks.Task<TodoItem?> GetByIdAsync(int id)
    {
        return await db.TodoItems.FindAsync(id);
    }

    public async System.Threading.Tasks.Task<bool> CompleteAsync(int id)
    {
        var item = await db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await db.SaveChangesAsync();
        return true;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var item = await db.TodoItems.FindAsync(id);
        if (item == null) return false;
        db.TodoItems.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }
}