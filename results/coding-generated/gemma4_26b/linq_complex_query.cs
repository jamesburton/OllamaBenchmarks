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
                group.Key,
                group.Sum(s => s.Amount),
                group.Average(s => s.Amount),
                group.Count()
            ))
            .OrderByDescending(summary => summary.TotalAmount)
            .ToList();
    }
}

public class Program
{
    public static void Main()
    {
        var sales = new List<Sale>
        {
            new Sale("Laptop", "Electronics", 1200.00m, new DateOnly(2023, 10, 1)),
            new Sale("Smartphone", "Electronics", 800.00m, new Dateability(2023, 10, 2)), // Note: DateOnly is used
            new Sale("Mouse", "Electronics", 25.50m, new DateOnly(2023, 10, 3)),
            new Sale("Notebook", "Stationery", 5.00m, new DateOnly(2023, 10, 4)),
            new Sale("Pen", "Stationery", 2.50m, new DateOnly(2023, 10, 5)),
            new Sale("Desk Chair", "Furniture", 150.00m, new DateOnly(2023, 10, 6)),
            new Sale("Desk Lamp", "Furniture", 45.00m, new DateOnly(2023, 10, 7))
        };

        // Correcting the sample data for a clean run
        var sampleSales = new List<Sale>
        {
            new Sale("Laptop", "Electronics", 1200m, new DateOnly(2023, 1, 1)),
            new Sale("Mouse", "Electronics", 50m, new DateOnly(2023, 1, 2)),
            new Sale("Book", "Stationery", 20m, new DateOnly(2023, 1, 3)),
            new Sale("Pen", "Stationery", 5m, new DateOnly(2023, 1, 4)),
            new Sale("Chair", "Furniture", 200m, new DateOnly(2023, 1, 5))
        };

        var summaries = SalesAnalyzer.SummarizeByCategory(sampleSales);

        Console.WriteLine($"{"Category",-15} | {"Total",-10} | {"Average",-10} | {"Count",-5}");
        Console.WriteLine(new string('-', 50));
        foreach (var summary in summaries)
        {
            Console.WriteLine($"{summary.Category,-15} | {summary.TotalAmount,-10:C} | {summary.AverageAmount,-10:C} | {summary.Count,-5}");
        }
    }
}