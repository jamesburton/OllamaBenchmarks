using Microsoft.EntityFrameworkCore;

public class Task_
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public int OwnerId { get; set; }
}

public class TodoContext : DbContext
{
    public DbSet<Task_> Tasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("TodoDatabase");
    }
}

public static class TodoRepository
{
    public static (int Total, int Completed) GetProgress(TodoContext context, int ownerId)
    {
        var tasks = context.Tasks.Where(t => t.OwnerId == ownerId);
        int total = tasks.Count();
        int completed = tasks.Count(t => t.IsComplete);
        return (total, completed);
    }
}