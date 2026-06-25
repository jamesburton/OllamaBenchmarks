using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;

namespace TodoService
{
    public class TodoItem : IEquatable<TodoItem>
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsComplete { get; set; }
        public DateTime CreatedAt { get; set; }

        public static implicit operator TodoItem(int id)
        {
            return new TodoItem
            {
                Id = id,
                Title = "Todo item",
                IsComplete = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public bool Equals(TodoItem other) => ReferenceEquals(this, other) || Id == other.Id && string.Equals(Title, other.Title);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TodoItem) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id.GetHashCode() * 397) ^ Title.GetHashCode());
            }
        }

        public static readonly TodoItem Null = default(TodoItem);

        public async Task CreateAsync(string title)
        {
            using var db = new TodoDbContext();
            var item = new TodoItem { Title = title };
            await db.TodoItems.AddAsync(item);
            await db.SaveChangesAsync();
        }

        public async Task<TodoItem> GetByIdAsync(int id) => await db.TodoItems.FindAsync(id);

        public async Task<List<TodoItem>> GetAllAsync() => await db.TodoItems.ToListAsync();

        public async Task CompleteAsync(int id)
        {
            using var db = new TodoDbContext();
            var item = await db.TodoItems.FindAsync(id);
            if (item != null) item.IsComplete = true;
            await db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var db = new TodoDbContext();
            var item = await db.TodoItems.FindAsync(id);
            if (item != null) db.TodoItems.Remove(item);
            await db.SaveChangesAsync();
        }
    }

    public class TodoDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase("TestDb");
        }
    }
}