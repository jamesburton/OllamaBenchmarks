public class OrderServiceTests
{
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService();
    }

    [Fact]
    public void CalculateTotal_ShouldReturnCorrectTotal()
    {
        var subtotal = 100m;
        var taxRate = 0.08;
        var expected = 108m;

        var result = _sut.CalculateTotal(subtotal, taxRate);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100m, 0.08, 108m)]
    [InlineData(50m, 0.1, 55m)]
    [InlineData(200m, 0.05, 210m)]
    public void CalculateTotal_ShouldHandleMultipleCases(decimal subtotal, double taxRate, decimal expected)
    {
        var result = _sut.CalculateTotal(subtotal, taxRate);

        result.Should().Be(expected);
    }

    [Fact]
    public void FormatOrderId_ShouldFormatCorrectly()
    {
        var id = 42;
        var expected = "ORD-000042";

        var result = _sut.FormatOrderId(id);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1, "ORD-000001")]
    [InlineData(123, "ORD-000123")]
    [InlineData(9999, "ORD-009999")]
    public void FormatOrderId_ShouldHandleMultipleIds(int id, string expected)
    {
        var result = _sut.FormatOrderId(id);

        result.Should().Be(expected);
    }

    [Fact]
    public void IsValidDiscount_ShouldReturnTrueForValidDiscount()
    {
        var discount = 0.25;

        var result = _sut.IsValidDiscount(discount);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.25)]
    [InlineData(0.5)]
    public void IsValidDiscount_ShouldReturnTrueForValidDiscounts(double discount)
    {
        var result = _sut.IsValidDiscount((decimal)discount);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidDiscount_ShouldReturnFalseForInvalidDiscount()
    {
        var discount = 0.6m;

        var result = _sut.IsValidDiscount(discount);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(0.6)]
    [InlineData(1.0)]
    public void IsValidDiscount_ShouldReturnFalseForInvalidDiscounts(double discount)
    {
        var result = _sut.IsValidDiscount((decimal)discount);

        result.Should().BeFalse();
    }

    [Fact]
    public void ApplyDiscount_ShouldApplyDiscountWhenValid()
    {
        var total = 100m;
        var discount = 0.2m;
        var expected = 80m;

        var result = _sut.ApplyDiscount(total, discount);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(100m, 0.1, 90m)]
    [InlineData(200m, 0.05, 190m)]
    [InlineData(50m, 0.0, 50m)]
    public void ApplyDiscount_ShouldHandleMultipleCases(decimal total, decimal discount, decimal expected)
    {
        var result = _sut.ApplyDiscount(total, discount);

        result.Should().Be(expected);
    }

    [Fact]
    public void ApplyDiscount_ShouldThrowForInvalidDiscount()
    {
        var total = 100m;
        var discount = 0.6m;

        Action act = () => _sut.ApplyDiscount(total, discount);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(100m, 0.6)]
    [InlineData(200m, 1.0)]
    [InlineData(50m, -0.1)]
    public void ApplyDiscount_ShouldThrowForInvalidDiscounts(decimal total, decimal discount)
    {
        Action act = () => _sut.ApplyDiscount(total, discount);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(CombinatorialData))]
    public void CombinatorialTests(decimal subtotal, double taxRate, int id, decimal discount)
    {
        var total = _sut.CalculateTotal(subtotal, taxRate);
        var orderId = _sut.FormatOrderId(id);
        var isValid = _sut.IsValidDiscount(discount);

        total.Should().BeGreaterThan(0);
        orderId.Should().NotBeNullOrEmpty();
        isValid.Should().BeTrue();
    }

    public static IEnumerable<object[]> CombinatorialData()
    {
        yield return new object[] { 100m, 0.08, 42, 0.25m };
        yield return new object[] { 50m, 0.1, 123, 0.0m };
        yield return new object[] { 200m, 0.05, 9999, 0.5m };
    }
}