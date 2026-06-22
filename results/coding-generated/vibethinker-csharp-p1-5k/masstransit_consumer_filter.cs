using MassTransit;

namespace Contracts;
{
            public static Record ProcessPayment(string currency, decimal amount)
            {
                return new ProcessPayment
                {
                    OrderId = Guid.NewGuid(),
                    Amount = amount,
                    Currency = currency
                };
            }

            public static Record PaymentProcessed(string currency, DateTime processedAt)
            {
                return new PaymentProcessed
                {
                    OrderId = Guid.NewGuid(),
                    ProcessedAt = processedAt
                };
            }
        }