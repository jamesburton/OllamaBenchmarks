using System;
using System.Collections.Generic;
using System.Linq;

// 1. Define the Sale record
public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

// 2. Define the CategorySummary record
public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

// 3. Define the static class with the summarization method
public static class SalesAnalyzer
{
    /// <summary>
    /// Summarizes sales data by category, calculating total, average, and count, and orders the results.
    /// </summary>
    /// <param name="sales">The collection of Sale records.</param>
    /// <returns>A list of CategorySummary records, ordered by TotalAmount descending.</returns>
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales
            // Group sales by Category
            .GroupBy(s => s.Category)
            // Calculate the summary for each group
            .Select(g => new CategorySummary(
                Category: g.Key,
                TotalAmount: g.Sum(s => s.Amount),
                AverageAmount: g.Average(s => s.Amount),
                Count: g.Count()
            ))
            // Order by TotalAmount descending
            .OrderByDescending(cs => cs.TotalAmount)
            // Convert the result to a List
            .ToList();
    }
}

// Example Usage (Optional, for testing purposes)
public class Program
{
    public static void Main()
    {
        // Sample Data
        var salesData = new List<Sale>
        {
            new Sale("Laptop", "Electronics", 1200.50m, new DateOnly(2023, 10, 1)),
            new Sale("T-Shirt", "Apparel", 25.00m, new DateOnly(2023, 10, 2)),
            new Sale("Mouse", "Electronics", 45.99m, new DateOnly(2023, 10, 3)),
            new Sale("Jeans", "Apparel", 75.50m, new DateOnly(2023, 10, 4)),
            new Sale("Keyboard", "Electronics", 89.99m, new DateOnly(2023, 10, 5)),
            new Sale("Socks", "Apparel", 10.00m, new DateOnly(2023, 10, 6))
        };

        // Analyze the data
        var summaries = SalesAnalyzer.SummarizeByCategory(salesData);

        // Output the results
        Console.WriteLine("--- Category Sales Summary ---");
        foreach (var summary in summaries)
        {
            Console.WriteLine($"Category: {summary.Category}");
            Console.WriteLine($"  Total Amount: {summary.TotalAmount:C}");
            Console.WriteLine($"  Average Amount: {summary.AverageAmount:C}");
            Console.WriteLine($"  Count: {summary.Count}");
            Console.WriteLine("-----------------------------");
        }
    }
}