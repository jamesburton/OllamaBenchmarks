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
    private readonly OrderService _service = new();

    [Fact]
    public void CalculateTotal_ReturnsCorrectTotal_WithPositiveTax()
    {
        var result = _service.CalculateTotal(100m, 0.2m);
        result.Should().Be(120m);
    }

    [Fact]
    public void CalculateTotal_ReturnsZero_WhenSubtotalIsZero()
    {
        var result = _service.CalculateTotal(0m, 0.1m);
        result.Should().Be(0m);
    }

    [Fact]
    public void FormatOrderId_ReturnsCorrectFormat()
    {
        var result = _service.FormatOrderId(42);
        result.Should().Be("ORD-000042");
    }

    [Fact]
    public void IsValidDiscount_ReturnsTrue_ForValidDiscount()
    {
        var result = _service.IsValidDiscount(0.25m);
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidDiscount_ReturnsFalse_ForNegativeDiscount()
    {
        var result = _service.IsValidDiscount(-0.1m);
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidDiscount_ReturnsFalse_ForDiscountGreaterThan50Percent()
    {
        var result = _service.IsValidDiscount(0.51m);
        result.Should().BeFalse();
    }

    [Fact]
    public void ApplyDiscount_ThrowsArgumentException_ForInvalidDiscount()
    {
        Action act = () => _service.ApplyDiscount(100m, 0.6m);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Invalid discount");
    }

    [Theory]
    [InlineData(100m, 0.2m, 120m)]
    [InlineData(50m, 0.1m, 55m)]
    [InlineData(0m, 0.25m, 0m)]
    public void CalculateTotal_WithInlineData_ReturnsExpectedResult(decimal subtotal, double taxRateDouble, decimal expected)
    {
        var taxRate = (decimal)taxRateDouble;
        var result = _service.CalculateTotal(subtotal, taxRate);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0m, true)]
    [InlineData(0.5m, true)]
    [InlineData(-0.01m, false)]
    [InlineData(0.51m, false)]
    public void IsValidDiscount_WithInlineData_ReturnsExpectedResult(decimal discount, bool expected)
    {
        var result = _service.IsValidDiscount(discount);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100m, 0.2m, 80m)]
    [InlineData(50m, 0.5m, 25m)]
    [InlineData(0m, 0.3m, 0m)]
    public void ApplyDiscount_WithInlineData_ReturnsExpectedResult(decimal total, double discountDouble, decimal expected)
    {
        var discount = (decimal)discountDouble;
        var result = _service.ApplyDiscount(total, discount);
        result.Should().Be(expected);
    }

    public static TheoryData<decimal, decimal, decimal> ApplyDiscountTestData =>
        new()
        {
            { 200m, 0.1m, 180m },
            { 75m, 0.25m, 56.25m }
        };

    [Theory]
    [MemberData(nameof(ApplyDiscountTestData))]
    public void ApplyDiscount_WithMemberData_ReturnsExpectedResult(decimal total, decimal discount, decimal expected)
    {
        var result = _service.ApplyDiscount(total, discount);
        result.Should().Be(expected);
    }

    public static MatrixTheoryData<decimal, decimal, decimal> CalculateTotalCombinatorialData =>
        new(
            new[] { 100m, 200m },
            new[] { 0.1m, 0.2m },
            new[] { 110m, 220m }
        );

    [Theory]
    [MemberData(nameof(CalculateTotalCombinatorialData))]
    public void CalculateTotal_CombinatorialTest(decimal subtotal, decimal taxRate, decimal expected)
    {
        var result = _service.CalculateTotal(subtotal, taxRate);
        result.Should().Be(expected);
    }

    [Fact]
    public void MultipleAssertions_TestAllCalculations()
    {
        var subtotal = 100m;
        var taxRate = 0.2m;
        var expectedTotal = 120m;
        var discount = 0.1m;
        var expectedDiscountedTotal = 108m;

        Assert.Multiple(
            () => _service.CalculateTotal(subtotal, taxRate).Should().Be(expectedTotal),
            () => _service.ApplyDiscount(expectedTotal, discount).Should().Be(expectedDiscountedTotal)
        );
    }

    [Theory]
    [InlineData(1, "ORD-000001")]
    [InlineData(123, "ORD-000123")]
    [InlineData(999999, "ORD-999999")]
    public void FormatOrderId_WithInlineData_ReturnsExpectedFormat(int id, string expected)
    {
        var result = _service.FormatOrderId(id);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(0m, true)]
    [InlineData(0.5m, true)]
    [InlineData(-0.0001m, false)]
    [InlineData(0.5001m, false)]
    public void IsValidDiscount_BoundaryValues(decimal discount, bool expected)
    {
        var result = _service.IsValidDiscount(discount);
        result.Should().Be(expected);
    }

    [Fact]
    public void ApplyDiscount_WithZeroDiscount_ReturnsOriginalTotal()
    {
        var result = _service.ApplyDiscount(100m, 0m);
        result.Should().Be(100m);
    }

    [Fact]
    public void ApplyDiscount_WithMaximumValidDiscount_ReturnsHalfTotal()
    {
        var result = _service.ApplyDiscount(100m, 0.5m);
        result.Should().Be(50m);
    }
}