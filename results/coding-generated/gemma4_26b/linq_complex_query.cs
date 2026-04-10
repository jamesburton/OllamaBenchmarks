using System;
using System.Collections.Generic;
using System.Linq;

public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

public static class SalesAnalyzer
{
    public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
    {
        return sales
            .GroupBy(s => s.Category)
            .Select(group => new CategorySummary(
                Category: group.Key,
                TotalAmount: group.Sum(s => s.Amount),
                AverageAmount: group.Average(s => s.Amount),
                Count: group.Count()
            ))
            .OrderByDescending(summary => summary.TotalAmount)
            .ToList();
    }
}

// Optional: Program class to demonstrate functionality
public class Program
{
    public static void Main()
    {
        var sales = new List<Sale>
        {
            new Sale("Laptop", "Electronics", 1200.00m, new DateOnly(2023, 10, 1)),
            new Sale("Mouse", "Electronics", 25.50m, new DateOnly(2023, 10, 2)),
            new Sale("Desk Chair", "Furniture", 150.00m, new DateOnly(2023, 10, 3)),
            new Sale("Monitor", "Electronics", 300.00m, new DateOnly(2023, 10, 4)),
            new Sale("Notebook", "Stationery", 5.00m, new DateOnly(2023, 10, 5)),
            new Sale("Lamp", "Furniture", 45.00m, new DateOnly(2023, 10, 6))
        };

        var summaries = SalesAnalyzer.SummarizeByCategory(sales);

        Console.WriteLine($"{"Category",-15} | {"Total",-10} | {"Average",-10} | {"Count",-5}");
        Console.WriteLine(new string('-', 50));
        foreach (var summary in summaries)
        {
            Console.WriteLine($"{summary.Category,-15} | {summary.TotalAmount,-10:C} | {summary.AverageAmount,-10:C} | {summary.Count,-5}");
        }
    }
}