using System;
using Xunit;
using AwesomeAssertions;

public class OrderServiceTests
{
    private readonly OrderService _orderService = new();

    [Fact]
    public void CalculateTotal_ValidInputs_ReturnsCorrectTotal()
    {
        // Arrange
        var subtotal = 100.0m;
        var taxRate = 0.08m;

        // Act
        var total = _orderService.CalculateTotal(subtotal, taxRate);

        // Assert
        Assert.Equal(108.0m, total);
    }

    [Theory]
    [InlineData(100, 0.08, 108)]
    [InlineData(50, 0.10, 55)]
    [InlineData(200, 0.05, 210)]
    public void CalculateTotal_ValidInputs_ReturnsCorrectTotal_Theory(decimal subtotal, decimal taxRate, decimal expected)
    {
        // Arrange
        // No need to repeat setup for each test case

        // Act
        var total = _orderService.CalculateTotal(subtotal, taxRate);

        // Assert
        Assert.Equal(expected, total);
    }

    [Fact]
    public void FormatOrderId_ValidId_ReturnsFormattedOrderId()
    {
        // Arrange
        var id = 42;

        // Act
        var formattedId = _orderService.FormatOrderId(id);

        // Assert
        Assert.Equal("ORD-000042", formattedId);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(0.5m, true)]
    [InlineData(-0.1m, false)]
    public void IsValidDiscount_ValidDiscount_ReturnsExpectedResult(decimal discount, bool expected)
    {
        // Arrange
        // No need to repeat setup for each test case

        // Act
        var isValid = _orderService.IsValidDiscount(discount);

        // Assert
        Assert.Equal(expected, isValid);
    }

    [Theory]
    [InlineData(100, 0.1, 90)]
    [InlineData(200, 0.25m, 150)]
    [InlineData(50, 0.5m, 25)]
    public void ApplyDiscount_ValidDiscount_ReturnsDiscountedTotal(decimal total, decimal discount, decimal expected)
    {
        // Arrange
        // No need to repeat setup for each test case

        // Act
        var discountedTotal = _orderService.ApplyDiscount(total, discount);

        // Assert
        Assert.Equal(expected, discountedTotal);
    }

    [Fact]
    public void ApplyDiscount_InvalidDiscount_ThrowsArgumentException()
    {
        // Arrange
        var total = 100.0m;
        var discount = -0.1m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _orderService.ApplyDiscount(total, discount));
    }

    [Fact]
    public void FormatOrderId_NegativeId_ThrowsArgumentException()
    {
        // Arrange
        var id = -1;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _orderService.FormatOrderId(id));
    }

    [Theory]
    [MemberData(nameof(Data))]
    public void FormatOrderId_ValidId_ReturnsFormattedOrderId_Theory(int id, string expected)
    {
        // Arrange
        // No need to repeat setup for each test case

        // Act
        var formattedId = _orderService.FormatOrderId(id);

        // Assert
        Assert.Equal(expected, formattedId);
    }

    public static TheoryData<int, string> Data =>
        new TheoryData<int, string>
        {
            { 1, "ORD-000001" },
            { 42, "ORD-000042" },
            { 123456, "ORD-123456" }
        };

    [Fact]
    public void ApplyDiscount_InvalidDiscount_ThrowsArgumentException_Multiple()
    {
        // Arrange
        var total = 100.0m;
        var invalidDiscounts = new decimal[] { -0.1m, 1.1m };

        // Act & Collect All Failures
        Assert.Multiple(
            () => Assert.Throws<ArgumentException>(() => _orderService.ApplyDiscount(total, invalidDiscounts[0])),
            () => Assert.Throws<ArgumentException>(() => _orderService.ApplyDiscount(total, invalidDiscounts[1]))
        );
    }

    [Theory]
    [MatrixTheoryData(1, "a"), MatrixTheoryData(2, "b")]
    public void Test(int n, string s)
    {
        // Arrange
        // No need to repeat setup for each test case

        // Act

        // Assert
        Assert.True(true); // Placeholder assertion
    }

    public static TheoryData<int, string> MatrixTheoryData(int n, string s) =>
        new TheoryData<int, string>
        {
            { 1, "a" },
            { 2, "b" }
        };
}