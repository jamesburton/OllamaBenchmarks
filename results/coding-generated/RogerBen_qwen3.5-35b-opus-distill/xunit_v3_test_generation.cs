using System;
using Xunit;
using AwesomeAssertions;

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
    // Single test case with [Fact]
    [Fact]
    public void CalculateTotal_WithZeroSubtotal_ReturnsZero()
    {
        var service = new OrderService();
        var result = service.CalculateTotal(0m, 0.1m);
        result.Should().Be(0m);
    }

    // Parameterized test with [Theory] and [InlineData]
    [Theory]
    [InlineData(100m, 0.1m, 110m)]
    [InlineData(50m, 0.2m, 60m)]
    [InlineData(200m, 0.05m, 210m)]
    public void CalculateTotal_WithVariousInputs_ReturnsCorrectTotal(decimal subtotal, decimal taxRate, decimal expected)
    {
        var service = new OrderService();
        var result = service.CalculateTotal(subtotal, taxRate);
        result.Should().Be(expected);
    }

    // Single test case with [Fact]
    [Fact]
    public void FormatOrderId_WithValidId_ReturnsFormattedString()
    {
        var service = new OrderService();
        var result = service.FormatOrderId(123);
        result.Should().Be("ORD-000123");
    }

    // Parameterized test with [Theory] and [InlineData]
    [Theory]
    [InlineData(1, "ORD-000001")]
    [InlineData(123, "ORD-000123")]
    [InlineData(999999, "ORD-999999")]
    public void FormatOrderId_WithVariousIds_ReturnsCorrectFormat(int id, string expected)
    {
        var service = new OrderService();
        var result = service.FormatOrderId(id);
        result.Should().Be(expected);
    }

    // Single test case with [Fact]
    [Fact]
    public void IsValidDiscount_WithZeroDiscount_ReturnsTrue()
    {
        var service = new OrderService();
        var result = service.IsValidDiscount(0m);
        result.Should().BeTrue();
    }

    // Parameterized test with [Theory] and [InlineData]
    [Theory]
    [InlineData(0m, true)]
    [InlineData(0.25m, true)]
    [InlineData(0.5m, true)]
    [InlineData(-0.1m, false)]
    [InlineData(0.6m, false)]
    public void IsValidDiscount_WithVariousDiscounts_ReturnsCorrectResult(decimal discount, bool expected)
    {
        var service = new OrderService();
        var result = service.IsValidDiscount(discount);
        result.Should().Be(expected);
    }

    // Single test case with [Fact]
    [Fact]
    public void ApplyDiscount_WithValidDiscount_ReturnsDiscountedTotal()
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(100m, 0.1m);
        result.Should().Be(90m);
    }

    // Parameterized test with [Theory] and [InlineData]
    [Theory]
    [InlineData(100m, 0.1m, 90m)]
    [InlineData(200m, 0.2m, 160m)]
    [InlineData(50m, 0.5m, 25m)]
    public void ApplyDiscount_WithValidDiscounts_ReturnsCorrectTotal(decimal total, decimal discount, decimal expected)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(expected);
    }

    // Test with Assert.Multiple() to collect all failures
    [Fact]
    public void ApplyDiscount_WithMultipleAssertions_CollectsAllFailures()
    {
        var service = new OrderService();

        Assert.Multiple(() =>
        {
            service.ApplyDiscount(100m, 0.1m).Should().Be(90m);
            service.ApplyDiscount(200m, 0.2m).Should().Be(160m);
            service.ApplyDiscount(50m, 0.5m).Should().Be(25m);
        });
    }

    // MatrixTheoryData for combinatorial tests
    public static MatrixTheoryData<decimal, decimal> DiscountTestData =>
        new([100m, 200m], [0.1m, 0.2m, 0.5m]);

    [Theory, MemberData(nameof(DiscountTestData))]
    public void ApplyDiscount_CombinatorialTest(decimal total, decimal discount)
    {
        var service = new OrderService();
        var expected = total * (1 - discount);
        service.ApplyDiscount(total, discount).Should().Be(expected);
    }

    // Test for exception throwing
    [Fact]
    public void ApplyDiscount_WithInvalidDiscount_ThrowsArgumentException()
    {
        var service = new OrderService();
        Action act = () => service.ApplyDiscount(100m, -0.1m);
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid discount*");
    }

    // Additional parameterized test for exception throwing
    [Theory]
    [InlineData(-0.1m)]
    [InlineData(0.6m)]
    public void ApplyDiscount_WithInvalidDiscounts_ThrowsArgumentException(decimal discount)
    {
        var service = new OrderService();
        Action act = () => service.ApplyDiscount(100m, discount);
        act.Should().Throw<ArgumentException>().WithMessage("*Invalid discount*");
    }
}