using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
}

public class ReviewContext : DbContext
{
    public DbSet<Review> Reviews { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("ReviewDatabase");
    }
}

public static class ReviewRepository
{
    public static double GetAverageRating(ReviewContext context, int productId)
    {
        var reviews = context.Reviews.Where(r => r.ProductId == productId);

        if (!reviews.Any())
        {
            return 0;
        }

        return reviews.Average(r => r.Rating);
    }
}