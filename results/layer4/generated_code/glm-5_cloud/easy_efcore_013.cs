using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public class Inventory
{
    public int Id { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class InventoryContext : DbContext
{
    public DbSet<Inventory> Items { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("InventoryDb");
    }
}

public static class InventoryRepository
{
    public static decimal GetTotalValue(InventoryContext context)
    {
        return context.Items.Sum(i => i.Quantity * i.UnitPrice);
    }
}