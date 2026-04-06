using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;
}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}

using Microsoft.EntityFrameworkCore;

public class TodoItem {
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = default!;

}

public class TodoDbContext : DbContext {
    private readonly DbContextOptions<TodoDbContext> _options;

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.CreatedAt)
            .HasDefaultValueSql(DateTime.UtcNow);
    }
}

public interface ITodoService {
    Task<TodoItem?> GetByIdAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> DeleteAsync(int id);
}

public class TodoService : ITodoService {
    private readonly TodoDbContext _db;

    public TodoService(TodoDbContext db) => _db = db;

    public async Task<TodoItem?> GetByIdAsync(int id) =>
        await _db.TodoItems.FindAsync(id);

    public async Task<bool> CompleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        item.IsComplete = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id) {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return false;
        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        return true;
    }
}