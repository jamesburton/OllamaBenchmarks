public record PriceTier(string Name, decimal MinAmount, decimal MaxAmount, decimal Multiplier);

public class PricingEngine
{
    private readonly List<PriceTier> _tiers = new();

    public void AddTier(PriceTier tier)
    {
        _tiers.Add(tier);
    }

    public decimal CalculatePrice(decimal baseAmount)
    {
        foreach (var tier in _tiers)
        {
            if (baseAmount >= tier.MinAmount && baseAmount <= tier.MaxAmount)
            {
                return baseAmount * tier.Multiplier;
            }
        }

        return baseAmount;
    }
}