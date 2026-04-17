using Microsoft.EntityFrameworkCore;

public class Permission
{
    public int Id { get; set; }
    public string RoleName { get; set; }
    public string Resource { get; set; }
    public string Action { get; set; }
}

public class PermContext : DbContext
{
    public DbSet<Permission> Permissions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("PermDb");
    }
}

public static class PermissionRepository
{
    public static bool HasPermission(PermContext context, string roleName, string resource, string action)
    {
        return context.Permissions.Any(p => 
            p.RoleName == roleName && 
            p.Resource == resource && 
            p.Action == action);
    }
}