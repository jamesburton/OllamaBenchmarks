using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IStockService
{
    Task<decimal> GetPriceAsync(string ticker);
}

public class PortfolioCalculator
{
    private readonly IStockService _stockService;

    public PortfolioCalculator(IStockService stockService)
    {
        _stockService = stockService ?? throw new ArgumentNullException(nameof(stockService));
    }

    public async Task<decimal> GetTotalValueAsync(Dictionary<string, int> holdings)
    {
        if (holdings == null || holdings.Count == 0)
        {
            return 0m;
        }

        decimal totalValue = 0m;

        foreach (var holding in holdings)
        {
            string ticker = holding.Key;
            int quantity = holding.Value;

            decimal price = await _stockService.GetPriceAsync(ticker);
            totalValue += price * quantity;
        }

        return totalValue;
    }
}