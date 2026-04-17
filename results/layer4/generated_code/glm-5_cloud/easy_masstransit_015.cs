using MassTransit;
using System.Collections.Generic;

namespace Contracts
{
    public record StockAlert(string Symbol, decimal Price, string AlertType);
}

public class StockAlertConsumer : IConsumer<Contracts.StockAlert>
{
    public List<string> BuyAlerts { get; set; } = new();
    public List<string> SellAlerts { get; set; } = new();

    public Task Consume(ConsumeContext<Contracts.StockAlert> context)
    {
        var message = context.Message;

        if (message.AlertType == "buy")
        {
            BuyAlerts.Add(message.Symbol);
        }
        else if (message.AlertType == "sell")
        {
            SellAlerts.Add(message.Symbol);
        }

        return Task.CompletedTask;
    }
}