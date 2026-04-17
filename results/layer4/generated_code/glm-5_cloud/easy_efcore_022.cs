using Microsoft.EntityFrameworkCore;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Discount { get; set; }
    public bool IsUsed { get; set; }
}

public class CouponContext : DbContext
{
    public DbSet<Coupon> Coupons { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("CouponDatabase");
    }
}

public static class CouponRepository
{
    public static Coupon? RedeemCoupon(CouponContext context, string code)
    {
        var coupon = context.Coupons
            .FirstOrDefault(c => !c.IsUsed && c.Code.ToLower() == code.ToLower());

        if (coupon == null)
        {
            return null;
        }

        coupon.IsUsed = true;
        context.SaveChanges();

        return coupon;
    }
}