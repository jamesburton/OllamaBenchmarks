using MassTransit;
namespace Contracts;

public static Record ProcessPayment(string currency, decimal amount)
{
    return new ProcessPayment
    {
        OrderId = Guid.NewGuid(),
        Amount = amount,
        Currency = currency
    };
}