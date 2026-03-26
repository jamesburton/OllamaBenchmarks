public class OrderService
{
    public decimal CalculateTotal(decimal subtotal, decimal taxRate) => subtotal * (1 + taxRate);
    public string FormatOrderId(int id) => $"ORD-{id:D6}";
    public bool IsValidDiscount(decimal discount) => discount >= 0 && discount <= 0.5m;
    public decimal ApplyDiscount(decimal total, decimal discount) =>
        IsValidDiscount(discount) ? total * (1 - discount) : throw new ArgumentException("Invalid discount");
}

public class OrderServiceTests
{
    [Fact]
    public void CalculateTotal_WithZeroSubtotalAndTax_ShouldReturnZero()
    {
        var service = new OrderService();
        var result = service.CalculateTotal(0m, 0.1);
        result.Should().Be(0m);
    }

    [Fact]
    public void FormatOrderId_WithValidId_ShouldFormatCorrectly()
    {
        var service = new OrderService();
        var formattedId = service.FormatOrderId(123456);
        formattedId.Should().Be("ORD-123456");
    }

    [Theory]
    [InlineData(100m, 0.1)]
    [InlineData(200m, 0.2)]
    public void CalculateTotal_WithVariousSubtotalsAndTaxRates_ShouldCalculateCorrectly(decimal subtotal, double taxRate)
    {
        var service = new OrderService();
        var result = service.CalculateTotal(subtotal, (decimal)taxRate);
        result.Should().Be(subtotal * (1 + (decimal)taxRate));
    }

    [Fact]
    public void ApplyDiscount_WithValidDiscount_ShouldApplyCorrectly()
    {
        var service = new OrderService();
        var total = 100m;
        var discount = 0.25m;
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(total * (1 - discount));
    }

    [Fact]
    public void ApplyDiscount_WithInvalidDiscount_ShouldThrowArgumentException()
    {
        var service = new OrderService();
        Action act = () => service.ApplyDiscount(100m, 0.6m);
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid discount*");
    }

    [Theory]
    [MemberData(nameof(DiscountTestData))]
    public void ApplyDiscount_WithVariousDiscounts_ShouldValidateCorrectly(decimal total, decimal discount, bool expectedIsValid)
    {
        var service = new OrderService();
        var isValid = service.IsValidDiscount(discount);
        isValid.Should().Be(expectedIsValid);
    }

    public static IEnumerable<object[]> DiscountTestData => new List<object[]>
    {
        new object[] { 100m, -0.1m, false },
        new object[] { 100m, 0m, true },
        new object[] { 100m, 0.5m, true },
        new object[] { 100m, 0.6m, false }
    };

    [Fact]
    public void ApplyDiscount_WithMultipleScenarios_ShouldCollectAllFailures()
    {
        var service = new OrderService();
        Assert.Multiple(() =>
        {
            Action act1 = () => service.ApplyDiscount(100m, -0.1m);
            act1.Should().Throw<ArgumentException>().WithMessage("*Invalid discount*");

            Action act2 = () => service.ApplyDiscount(200m, 0.6m);
            act2.Should().Throw<ArgumentException>().WithMessage("*Invalid discount*");
        });
    }

    public static MatrixTheoryData<decimal, decimal> DiscountMatrixData =>
        new MatrixTheoryData<decimal, decimal>(
            new[] { -0.1m, 0m, 0.5m, 0.6m },
            new[] { 100m, 200m }
        );

    [Theory]
    [MemberData(nameof(DiscountMatrixData))]
    public void ApplyDiscount_WithCombinatorialScenarios_ShouldValidateCorrectly(decimal discount, decimal total)
    {
        var service = new OrderService();
        if (service.IsValidDiscount(discount))
        {
            Action act = () => service.ApplyDiscount(total, discount);
            act.Should().NotThrow<ArgumentException>();
        }
        else
        {
            Action act = () => service.ApplyDiscount(total, discount);
            act.Should().Throw<ArgumentException>().WithMessage("*Invalid discount*");
        }
    }
}