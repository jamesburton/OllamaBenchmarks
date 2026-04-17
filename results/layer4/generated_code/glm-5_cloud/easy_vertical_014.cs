using System;
using System.Collections.Generic;
using System.Linq;

public record CouponCode(string Code, decimal DiscountPercent, DateTime Expiry);

public class CouponService
{
    private readonly List<CouponCode> _coupons = new();

    public void AddCoupon(CouponCode coupon)
    {
        _coupons.Add(coupon);
    }

    public decimal? ApplyCoupon(string code, DateTime now)
    {
        var coupon = _coupons.FirstOrDefault(c => c.Code == code);

        if (coupon == null)
        {
            return null;
        }

        if (coupon.Expiry < now)
        {
            return null;
        }

        return coupon.DiscountPercent;
    }

    public List<CouponCode> GetExpired(DateTime now)
    {
        return _coupons.Where(c => c.Expiry < now).ToList();
    }
}