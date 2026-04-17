using Microsoft.EntityFrameworkCore;

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public decimal Total { get; set; }
    public bool IsCompleted { get; set; }
}

public class OrderContext : DbContext
{
    public DbSet<Order> Orders { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("OrderDatabase");
    }
}

public static class OrderRepository
{
    public static decimal GetTotalRevenue(OrderContext context)
    {
        return context.Orders
            .Where(o => o.IsCompleted)
            .Sum(o => o.Total);
    }
}