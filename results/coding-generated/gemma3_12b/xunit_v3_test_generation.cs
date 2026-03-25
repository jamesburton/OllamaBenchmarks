using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;
using Xunit.Sdk;

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
    public void CalculateTotal_ValidInput_ReturnsCorrectTotal()
    {
        // Arrange
        var orderService = new OrderService();
        decimal subtotal = 100;
        decimal taxRate = 0.1m;

        // Act
        decimal actualTotal = orderService.CalculateTotal(subtotal, taxRate);

        // Assert
        Assert.Equal(110, actualTotal);
    }

    [Theory]
    [InlineData(1, "ORD-000001")]
    [InlineData(10, "ORD-000010")]
    [InlineData(100, "ORD-000100")]
    public void FormatOrderId_ValidId_ReturnsFormattedOrderId(int id, string expectedOrderId)
    {
        // Arrange
        var orderService = new OrderService();

        // Act
        string actualOrderId = orderService.FormatOrderId(id);

        // Assert
        Assert.Equal(expectedOrderId, actualOrderId);
    }

    [Fact]
    public void IsValidDiscount_ValidDiscount_ReturnsTrue()
    {
        // Arrange
        var orderService = new OrderService();
        decimal discount = 0.25m;

        // Act
        bool isValid = orderService.IsValidDiscount(discount);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void IsValidDiscount_InvalidDiscount_ReturnsFalse()
    {
        // Arrange
        var orderService = new OrderService();
        decimal discount = 0.6m;

        // Act
        bool isValid = orderService.IsValidDiscount(discount);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ApplyDiscount_ValidDiscount_ReturnsCorrectDiscountedTotal()
    {
        // Arrange
        var orderService = new OrderService();
        decimal total = 100;
        decimal discount = 0.1m;

        // Act
        decimal actualDiscountedTotal = orderService.ApplyDiscount(total, discount);

        // Assert
        Assert.Equal(90, actualDiscountedTotal);
    }

    [Fact]
    public void ApplyDiscount_InvalidDiscount_ThrowsArgumentException()
    {
        // Arrange
        var orderService = new OrderService();
        decimal total = 100;
        decimal discount = 0.6m;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => orderService.ApplyDiscount(total, discount));
    }

    [Fact]
    public void ApplyDiscount_ZeroDiscount_ReturnsOriginalTotal()
    {
        // Arrange
        var orderService = new OrderService();
        decimal total = 100;
        decimal discount = 0;

        // Act
        decimal actualDiscountedTotal = orderService.ApplyDiscount(total, discount);

        // Assert
        Assert.Equal(100, actualDiscountedTotal);
    }

    [Fact]
    public void ApplyDiscount_MaxDiscount_ReturnsCorrectDiscountedTotal()
    {
        // Arrange
        var orderService = new OrderService();
        decimal total = 100;
        decimal discount = 0.5m;

        // Act
        decimal actualDiscountedTotal = orderService.ApplyDiscount(total, discount);

        // Assert
        Assert.Equal(50, actualDiscountedTotal);
    }

    [Fact]
    public void MultipleAssertions()
    {
        // Arrange
        var orderService = new OrderService();
        decimal subtotal = 50;
        decimal taxRate = 0.05m;

        // Act
        decimal total = orderService.CalculateTotal(subtotal, taxRate);
        string orderId = orderService.FormatOrderId(123);

        // Assert
        Assert.Multiple(
            () => Assert.Equal(52.50, total),
            () => Assert.Equal("ORD-000123", orderId)
        );
    }

    public static class DiscountData
    {
        public static (decimal, decimal)[] GetDiscountData()
        {
            return new[]
            {
                (0, 100),
                (0.25m, 100),
                (0.5m, 100),
                (0.6m, 100)
            };
        }
    }

    [Theory, MemberData(nameof(DiscountData.GetDiscountData))]
    public void ApplyDiscount_ParameterizedTest(decimal discount, decimal total)
    {
        // Arrange
        var orderService = new OrderService();

        // Act & Assert
        if (discount <= 0.5m)
        {
            decimal expectedDiscountedTotal = total * (1 - discount);
            Assert.Equal(expectedDiscountedTotal, orderService.ApplyDiscount(total, discount));
        }
        else
        {
            Assert.Throws<ArgumentException>(() => orderService.ApplyDiscount(total, discount));
        }
    }

    public static class MatrixData
    {
        public static (decimal, decimal)[] GetMatrixData()
        {
            return new[]
            {
                (10, 0.1m),
                (20, 0.2m),
                (50, 0.05m)
            };
        }
    }

    [Theory, MemberData(nameof(MatrixData.GetMatrixData))]
    public void CalculateTotal_MatrixTheoryData(decimal subtotal, decimal taxRate)
    {
        // Arrange
        var orderService = new OrderService();

        // Act
        decimal actualTotal = orderService.CalculateTotal(subtotal, taxRate);
        decimal expectedTotal = subtotal * (1 + taxRate);

        // Assert
        Assert.Equal(expectedTotal, actualTotal);
    }
}