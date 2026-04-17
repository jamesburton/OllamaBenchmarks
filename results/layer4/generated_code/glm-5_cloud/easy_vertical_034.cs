using System;
using System.Collections.Generic;
using System.Linq;

public record Expense(string Id, string Category, decimal Amount, string PaidBy);

public class ExpenseTracker
{
    private readonly List<Expense> _expenses = new();

    public void AddExpense(Expense expense)
    {
        _expenses.Add(expense);
    }

    public decimal TotalByCategory(string category)
    {
        return _expenses
            .Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .Sum(e => e.Amount);
    }

    public Dictionary<string, decimal> TotalByPerson()
    {
        return _expenses
            .GroupBy(e => e.PaidBy)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
    }

    public List<Expense> GetAll()
    {
        return new List<Expense>(_expenses);
    }
}