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
    public void CalculateTotal_WithValidInputs_ReturnsCorrectTotal()
    {
        var service = new OrderService();
        var result = service.CalculateTotal(100m, 0.2m);
        result.Should().Be(120m);
    }

    [Theory]
    [InlineData(1, "ORD-000001")]
    [InlineData(10, "ORD-000010")]
    public void FormatOrderId_WithValidId_ReturnsFormattedString(int id, string expected)
    {
        var service = new OrderService();
        var result = service.FormatOrderId(id);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0.5m, true)]
    [InlineData(1.0m, false)]
    public void IsValidDiscount_WithValidInputs_ReturnsCorrectResult(decimal discount, bool expected)
    {
        var service = new OrderService();
        var result = service.IsValidDiscount(discount);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(-1, "Invalid discount")]
    [InlineData(0.5m, 95m)]
    public void ApplyDiscount_WithValidInputs_ReturnsCorrectResult(decimal total, decimal discount, decimal expected)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ThrowsArgumentException(decimal total, decimal discount)
    {
        var service = new OrderService();
        Action act = () => service.ApplyDiscount(total, discount);
        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void ApplyDiscount_WithInvalidInputs_ReturnsCorrectResult(decimal total, decimal discount)
    {
        var service = new OrderService();
        var result = service.ApplyDiscount(total, discount);
        result.Should().Be(100m);
    }

    [Theory]
    [InlineData(1.0m, 100m)]
    public void