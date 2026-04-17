using System;
using System.Collections.Generic;
using System.Linq;

public record Transaction(string Id, string AccountId, decimal Amount, string Type);

public class AccountLedger
{
    private readonly List<Transaction> _transactions = new List<Transaction>();

    public void Record(Transaction transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }
        _transactions.Add(transaction);
    }

    public decimal GetBalance(string accountId)
    {
        return _transactions
            .Where(t => t.AccountId == accountId)
            .Sum(t => t.Type.ToLower() == "credit" ? t.Amount : -t.Amount);
    }

    public List<Transaction> GetHistory(string accountId)
    {
        return _transactions
            .Where(t => t.AccountId == accountId)
            .ToList();
    }
}