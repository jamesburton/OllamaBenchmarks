using System;
using System.Collections.Generic;
using System.Linq;

namespace SalesAnalysis
{
    // Sale record
    public record Sale(string Product, string Category, decimal Amount, DateOnly Date);

    // CategorySummary record
    public record CategorySummary(string Category, decimal TotalAmount, decimal AverageAmount, int Count);

    public static class SalesAnalyzer
    {
        public static List<CategorySummary> SummarizeByCategory(IEnumerable<Sale> sales)
        {
            return sales
                .GroupBy(s => s.Category)
                .Select(g => new CategorySummary(
                    g.Key,
                    g.Sum(s => s.Amount),
                    g.Average(s => s.Amount),
                    g.Count()
                ))
                .OrderByDescending(s => s.TotalAmount)
                .ToList();
        }
    }

    // Example usage
    class Program
    {
        static void Main(string[] args)
        {
            var sales = new List<Sale>
            {
                new Sale("Laptop", "Electronics", 1200m, DateOnly.From(2023, 10, 15)),
                new Sale("Laptop", "Electronics", 800m, DateOnly.From(2023, 10, 20)),
                new Sale("Desk", "Furniture", 300m, DateOnly.From(2023, 10, 10)),
                new Sale("Chair", "Furniture", 150m, DateOnly.From(2023, 10, 12)),
                new Sale("Monitor", "Electronics", 600m, DateOnly.From(2023, 10, 18))
            };

            var summaries = SalesAnalyzer.SummarizeByCategory(sales);

            Console.WriteLine("Category Summary:");
            foreach (var summary in summaries)
            {
                Console.WriteLine($"{summary.Category}: Total=${summary.TotalAmount}, " +