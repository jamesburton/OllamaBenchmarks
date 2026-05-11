using System;
using System.Collections.Generic;
using System.Linq;

// 1. Sale record
public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

// 2. CategorySummary record
public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

// 3. Static class with the summarization method
public static class SalesAnalyzer
{
    /// <summary>
    /// Groups sales by category and calculates total, average, and count for each category.
    /// </summary>
    /// <param name="sales">The collection of Sale records.</param>
    /// <returns>A list of CategorySummary records, ordered by TotalAmount descending.</returns>
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales.GroupBy(s => s.Category)
            .Select(g => new CategorySummary(
                g.Key,                               // Category
                g.Sum(s => s.Amount),                // TotalAmount
                g.Average(s => s.Amount),            // AverageAmount
                g.Count()                           // Count
            ))
            .OrderByDescending(cs => cs.TotalAmount)
            .ToList();
    }
}

public class Program
{
    public static void Main()
    {
        // Sample Data
        var salesData = new List<Sale>
        {
            new Sale("Laptop", "Electronics", 1200.00m, new DateOnly(2023, 10, 1)),
            new Sale("T-Shirt", "Apparel", 25.50m, new DateOnly(2023, 10, 2)),
            new Sale("Mouse", "Electronics", 45.00m, new DateOnly(2023, 10, 3)),
            new Sale("Jeans", "Apparel", 75.00m, new DateOnly(2023, 10, 4)),
            new Sale("Keyboard", "Electronics", 75.00m, new DateOnly(2023, 10, 5)),
            new Sale("Socks", "Apparel", 10.00m, new DateOnly(2023, 10, 6))
        };

        Console.WriteLine("--- Sales Data ---");
        foreach (var sale in salesData)
        {
            Console.WriteLine($"Product: {sale.Product}, Category: {sale.Category}, Amount: {sale.Amount:C}");
        }
        Console.WriteLine("\n-----------------\n");


        // Analyze the data using the static method
        var summaries = SalesAnalyzer.SummarizeByCategory(salesData);

        Console.WriteLine("--- Category Summaries (Ordered by Total Amount Descending) ---");
        foreach (var summary in summaries)
        {
            Console.WriteLine($"Category: {summary.Category}");
            Console.WriteLine($"  Total Amount: {summary.TotalAmount:C}");
            Console.WriteLine($"  Average Amount: {summary.AverageAmount:C}");
            Console.WriteLine($"  Count: {summary.Count}");
            Console.WriteLine("--------------------------------------------------");
        }
    }
}