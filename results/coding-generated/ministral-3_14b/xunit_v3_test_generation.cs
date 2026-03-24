using AwesomeAssertions;
using Xunit;

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
    public void CalculateTotal_WithZeroTax_ReturnsSubtotal()
    {
        // Arrange
        decimal subtotal = 100m;
        decimal taxRate = 0m;

        // Act
        decimal result = _service.CalculateTotal(subtotal, taxRate);

        // Assert
        result.Should().Be(100m);
    }

    [Fact]
    public void CalculateTotal_WithPositiveTax_ReturnsSubtotalPlusTax()
    {
        // Arrange
        decimal subtotal = 100m;
        decimal taxRate = 0.1m;

        // Act
        decimal result = _service.CalculateTotal(subtotal, taxRate);

        // Assert
        result.Should().Be(110m);
    }

    [Fact]
    public void FormatOrderId_WithSingleDigit_ReturnsFormattedString()
    {
        // Arrange
        int id = 5;

        // Act
        string result = _service.FormatOrderId(id);

        // Assert
        result.Should().Be("ORD-000005");
    }

    [Fact]
    public void FormatOrderId_WithSixDigit_ReturnsFormattedString()
    {
        // Arrange
        int id = 123456;

        // Act
        string result = _service.FormatOrderId(id);

        // Assert
        result.Should().Be("ORD-123456");
    }

    [Fact]
    public void IsValidDiscount_WithZeroDiscount_ReturnsTrue()
    {
        // Arrange
        decimal discount = 0m;

        // Act
        bool result = _service.IsValidDiscount(discount);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidDiscount_WithHalfDiscount_ReturnsTrue()
    {
        // Arrange
        decimal discount = 0.5m;

        // Act
        bool result = _service.IsValidDiscount(discount);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidDiscount_WithNegativeDiscount_ReturnsFalse()
    {
        // Arrange
        decimal discount = -0.1m;

        // Act
        bool result = _service.IsValidDiscount(discount);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidDiscount_WithDiscountOverHalf_ReturnsFalse()
    {
        // Arrange
        decimal discount = 0.6m;

        // Act
        bool result = _service.IsValidDiscount(discount);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(0m, 0m)]
    [InlineData(0.1m, 0.1m)]
    [InlineData(0.5m, 0.5m)]
    public void ApplyDiscount_WithValidDiscount_ReturnsDiscountedTotal(decimal discount, decimal expectedDiscount)
    {
        // Arrange
        decimal total = 100m;

        // Act
        decimal result = _service.ApplyDiscount(total, discount);

        // Assert
        result.Should().Be(100m - (100m * expectedDiscount));
    }

    [Theory]
    [InlineData(-0.1m, "Invalid discount")]
    [InlineData(0.6m, "Invalid discount")]
    public void ApplyDiscount_WithInvalidDiscount_ThrowsArgumentException(decimal discount, string expectedMessage)
    {
        // Arrange
        decimal total = 100m;

        // Act & Assert
        Action act = () => _service.ApplyDiscount(total, discount);
        act.Should().Throw<ArgumentException>()
           .WithMessage(expectedMessage);
    }

    public static MatrixTheoryData<decimal, decimal, decimal> CalculateTotalTestData =>
        new([0m, 0.1m, 0.25m], [100m, 200m, 50m]);

    [Theory, MemberData(nameof(CalculateTotalTestData))]
    public void CalculateTotal_WithVariousValues_ReturnsCorrectTotal(decimal taxRate, decimal subtotal)
    {
        // Act
        decimal result = _service.CalculateTotal(subtotal, taxRate);

        // Assert
        result.Should().Be(subtotal * (1 + taxRate));
    }

    [Fact]
    public void CalculateTotal_WithMultipleFailures_CollectsAll()
    {
        // Arrange
        decimal subtotal = 100m;
        decimal taxRate = 0.1m;

        // Act
        decimal result = _service.CalculateTotal(subtotal, taxRate);

        // Assert - This will fail on purpose to demonstrate Assert.Multiple
        Assert.Multiple(() =>
        {
            result.Should().Be(100m); // This will fail
            result.Should().Be(110m); // This will pass
            result.Should().Be(120m); // This will fail
        });
    }

    [Fact]
    public void FormatOrderId_WithEdgeCases_ReturnsCorrectFormat()
    {
        // Arrange & Act & Assert
        Assert.Multiple(() =>
        {
            _service.FormatOrderId(0).Should().Be("ORD-000000");
            _service.FormatOrderId(999999).Should().Be("ORD-999999");
            _service.FormatOrderId(1).Should().Be("ORD-000001");
        });
    }
}