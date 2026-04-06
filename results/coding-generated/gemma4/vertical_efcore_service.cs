using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

// 1. Model
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 2. DbContext
public class TodoDbContext : DbContext
{
    public DbSet<TodoItem> TodoItems { get; set; }

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }
}

// 3. Interface
public interface ITodoService
{
    Task<TodoItem> CreateAsync(string title);
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

// 4. Implementation
public class TodoService : ITodoService
{
    private readonly TodoDbContext _context;

    public TodoService(TodoDbContext db)
    {
        _context = db;
    }

    public async Task<TodoItem> CreateAsync(string title)
    {
        var item = new TodoItem
        {
            Title = title,
            IsComplete = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.TodoItems.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await _context.TodoItems.ToListAsync();
    }

    public async Task<TodoItem?> GetByIdAsync(int id)
    {
        return await _context.TodoItems.FindAsync(id);
    }

    public async Task<bool> CompleteAsync(int id)
    {
        var item = await _context.TodoItems.FindAsync(id);
        if (item == null)
        {
            return false;
        }

        item.IsComplete = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var item = await _context.TodoItems.FindAsync(id);
        if (item == null)
        {
            return false;
        }

        _context.TodoItems.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
}