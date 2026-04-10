public class TodoItem {
            public int Id { get; set; }
            public string Title { get; set; }
            public bool IsComplete { get; set; }
            public DateTime CreatedAt { get; set; }
        }

public class TodoDbContext : DbContext {
            public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }
            public DbSet<TodoItem> TodoItems { get; set; }
        }

public interface ITodoService {
            Task<TodoItem> CreateAsync(string title);
            Task<List<TodoItem>> GetAllAsync();
            Task<TodoItem?> GetByIdAsync(int id);
            Task<bool> CompleteAsync(int id);
            Task<bool> DeleteAsync(int id);
        }

public class TodoService : ITodoService {
            private readonly TodoDbContext _db;
            public TodoService(TodoDbContext db) { _db = db; }
            // Implement methods...
        }