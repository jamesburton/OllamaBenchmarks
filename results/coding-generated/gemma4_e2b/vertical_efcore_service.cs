using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 1. TodoItem class
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; }
}

// 2. TodoDbContext class
public class TodoDbContext : DbContext
{
    public DbSet<TodoItem> TodoItems { get; set; }

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
    {
    }
}

// 3. ITodoService interface
public interface ITodoService
{
    Task<TodoItem> CreateAsync(string title);
    Task<List<TodoItem>> GetAllAsync();
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

// 4. TodoService class implementation
public class TodoService : ITodoService
{
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db)
    {
        _db = db;
    }

    public async Task<TodoItem> CreateAsync(string title)
    {
        var newItem = new TodoItem
        {
            Title = title,
            IsComplete = false,
            CreatedAt = DateTime.UtcNow
        };
        _db.TodoItems.Add(newItem);
        await _db.SaveChangesAsync();
        return newItem;
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

// Example usage context (for completeness, though not strictly required by prompt)
/*
public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup In-Memory Database
        var options = new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(databaseName: "TestTodoDb")
            .Options;

        using (var context = new TodoDbContext(options))
        {
            context.Database.EnsureCreated();

            // Setup Service
            var todoService = new TodoService(context);

            // Test Operations
            Console.WriteLine("--- Creating Item ---");
            var createdItem = await todoService.CreateAsync("Buy Groceries");
            Console.WriteLine($"Created: {createdItem.Title}, ID: {createdItem.Id}");

            Console.WriteLine("\n--- Getting All Items ---");
            var allItems = await todoService.GetAllAsync();
            foreach (var item in allItems)
            {
                Console.WriteLine($"ID: {item.Id}, Title: {item.Title}, Complete: {item.IsComplete}");
            }

            Console.WriteLine("\n--- Completing Item ---");
            bool completed = await todoService.CompleteAsync(createdItem.Id);
            Console.WriteLine($"Item {createdItem.Id} completed: {completed}");

            Console.WriteLine("\n--- Getting Updated Item ---");
            var updatedItem = await todoService.GetByIdAsync(createdItem.Id);
            Console.WriteLine($"Updated Status: {updatedItem?.IsComplete}");

            Console.WriteLine("\n--- Deleting Item ---");
            bool deleted = await todoService.DeleteAsync(createdItem.Id);
            Console.WriteLine($"Item {createdItem.Id} deleted: {deleted}");

            Console.WriteLine("\n--- Getting All Items After Deletion ---");
            var finalItems = await todoService.GetAllAsync();
            Console.WriteLine($"Total items remaining: {finalItems.Count}");
        }
    }
}
*/