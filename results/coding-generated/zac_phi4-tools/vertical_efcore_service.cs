using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        var todoItem = new TodoItem
        {
            Title = title,
            IsComplete = false,
            CreatedAt = DateTime.UtcNow
        };

        await _db.TodoItems.AddAsync(todoItem);
        await _db.SaveChangesAsync();

        return todoItem;
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
        var todoItem = await _db.TodoItems.FindAsync(id);

        if (todoItem == null) return false;

        todoItem.IsComplete = true;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var todoItem = await _db.TodoItems.FindAsync(id);

        if (todoItem == null) return false;

        _db.TodoItems.Remove(todoItem);
        await _db.SaveChangesAsync();

        return true;
    }
}