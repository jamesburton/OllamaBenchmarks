using System;

public record Money(decimal Amount, string Currency)
{
    public static Money operator +(Money a, Money b)
    {
        if (a is null)
            throw new ArgumentNullException(nameof(a));
        if (b is null)
            throw new ArgumentNullException(nameof(b));

        if (a.Currency != b.Currency)
        {
            throw new InvalidOperationException("Cannot add Money with different currencies.");
        }

        return new Money(a.Amount + b.Amount, a.Currency);
    }
}