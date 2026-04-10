using System;
using System.Collections.Generic;
using System.Linq;

// 1. A Sale record
public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

// 2. A CategorySummary record
public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

// 3. A static class for analysis
public static class SalesAnalyzer
{
    /// <summary>
    /// Summarizes sales data by category, calculating total, average, and count, and orders by total amount descending.
    /// </summary>
    /// <param name="sales">The collection of Sale records.</param>
    /// <returns>A list of CategorySummary records.</returns>
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales
            // Group sales by Category
            .GroupBy(sale => sale.Category)
            // For each group, calculate the summary statistics
            .Select(group => new CategorySummary(
                group.Key, // Category
                group.Sum(sale => sale.Amount), // TotalAmount
                group.Average(sale => sale.Amount), // AverageAmount
                group.Count() // Count
            ))
            // Order by TotalAmount descending
            .OrderByDescending(summary => summary.TotalAmount)
            // Convert the result to a List
            .ToList();
    }
}

public class Program
{
    public static void Main()
    {
        // Example Usage
        var salesData = new List<Sale>
        {
            new Sale("Laptop", "Electronics", 1200.50m, new DateOnly(2023, 10, 1)),
            new Sale("T-Shirt", "Apparel", 25.00m, new DateOnly(2023, 10, 2)),
            new Sale("Mouse", "Electronics", 35.99m, new DateOnly(2023, 10, 3)),
            new Sale("Jeans", "Apparel", 75.50m, new DateOnly(2023, 10, 4)),
            new Sale("Keyboard", "Electronics", 75.00m, new DateOnly(2023, 10, 5)),
            new Sale("Socks", "Apparel", 10.00m, new DateOnly(2023, 10, 6))
        };

        Console.WriteLine("--- Sales Data ---");
        foreach (var sale in salesData)
        {
            Console.WriteLine($"Product: {sale.Product}, Category: {sale.Category}, Amount: {sale.Amount:C}");
        }

        // Analyze the data
        var summaries = SalesAnalyzer.SummarizeByCategory(salesData);

        Console.WriteLine("\n--- Category Summaries (Ordered by Total Amount Descending) ---");
        if (summaries.Any())
        {
            foreach (var summary in summaries)
            {
                Console.WriteLine($"Category: {summary.Category}");
                Console.WriteLine($"  Total Amount: {summary.TotalAmount:C}");
                Console.WriteLine($"  Average Amount: {summary.AverageAmount:C}");
                Console.WriteLine($"  Count: {summary.Count}");
                Console.WriteLine("--------------------------------------------------");
            }
        }
        else
        {
            Console.WriteLine("No sales data to summarize.");
        }
    }
}