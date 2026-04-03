using System;
using System.Collections.Generic;
using System.Linq;

// 1. Sale record
public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

// 2. CategorySummary record
public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

// 3. Static class SalesAnalyzer
public static class SalesAnalyzer
{
    /// <summary>
    /// Groups sales by category, calculates summary statistics, and orders the results.
    /// </summary>
    /// <param name="sales">The collection of sales records.</param>
    /// <returns>A list of CategorySummary, ordered by TotalAmount descending.</returns>
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        // Use LINQ method syntax: GroupBy -> Select -> OrderByDescending
        return sales
            .GroupBy(sale => sale.Category) // Group by Category
            .Select(group => new CategorySummary(
                Category: group.Key,
                TotalAmount: group.Sum(sale => sale.Amount), // Sum
                AverageAmount: group.Average(sale => sale.Amount), // Average
                Count: group.Count() // Count
            ))
            .OrderByDescending(summary => summary.TotalAmount) // Order by TotalAmount descending
            .ToList();
    }
}

// Example usage (optional, but useful for testing the structure)
public class Program
{
    public static void Main()
    {
        var salesData = new List<Sale>
        {
            new Sale("Laptop X", "Electronics", 1200.00m, new DateOnly(2023, 10, 1)),
            new Sale("T-Shirt", "Apparel", 25.00m, new DateOnly(2023, 10, 2)),
            new Sale("Monitor Y", "Electronics", 300.00m, new DateOnly(2023, 10, 3)),
            new Sale("Jeans", "Apparel", 75.00m, new DateOnly(2023, 10, 4)),
            new Sale("Mouse Z", "Electronics", 50.00m, new DateOnly(2023, 10, 5)),
            new Sale("Sweater", "Apparel", 50.00m, new DateOnly(2023, 10, 6))
        };

        var summary = SalesAnalyzer.SummarizeByCategory(salesData);

        Console.WriteLine("--- Sales Summary by Category ---");
        foreach (var item in summary)
        {
            Console.WriteLine($"Category: {item.Category}");
            Console.WriteLine($"  Total Amount: {item.TotalAmount:C}");
            Console.WriteLine($"  Average Amount: {item.AverageAmount:C}");
            Console.WriteLine($"  Count: {item.Count}");
            Console.WriteLine("---------------------------------");
        }
    }
}