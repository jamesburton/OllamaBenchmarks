using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public DateTime LastModified { get; set; }
}

public class ProductDbContext : DbContext
{
    public DbSet<Product