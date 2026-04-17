using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class Transaction
{
    public int Id { get; set; }
    public string Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}

public class FinanceContext : DbContext
{
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("FinanceDatabase");
    }
}

public static class TransactionRepository
{
    public static (decimal Credits, decimal Debits) GetSummary(FinanceContext context)
    {
        var credits = context.Transactions
            .Where(t => t.Type == "credit")
            .Sum(t => t.Amount);

        var debits = context.Transactions
            .Where(t => t.Type == "debit")
            .Sum(t => t.Amount);

        return (credits, debits);
    }
}