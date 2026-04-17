using System;

public interface IDiscountService
{
    decimal GetDiscount(string promoCode);
}

public class PriceCalculator
{
    private readonly IDiscountService _discountService;

    public PriceCalculator(IDiscountService discountService)
    {
        _discountService = discountService;
    }

    public decimal CalculateFinalPrice(decimal basePrice, string promoCode)
    {
        decimal discount = _discountService.GetDiscount(promoCode);
        return basePrice - discount;
    }
}