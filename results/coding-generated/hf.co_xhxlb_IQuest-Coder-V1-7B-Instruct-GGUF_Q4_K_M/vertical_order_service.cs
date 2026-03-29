using OneOf;
using AwesomeAssertions;
using NSubstitute;
using Xunit;
using System;
using System.Collections.Generic;

namespace OrderService.Tests
{
    public partial class OrderServiceTests
    {
        private readonly IOrderRepository _repository;

        public OrderServiceTests()
        {
            _repository = Substitute.For<IOrderRepository>();
        }

        [Fact]
        public void CreateOrder_WithValidRequest_ReturnsOrder()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "John Doe",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10.0m },
                    new OrderItem { ProductName = "Gadget", Quantity = 1, UnitPrice = 20.0m }
                }
            };

            var service = new OrderService(_repository);

            // Act
            var result = service.CreateOrder(request);

            // Assert
            result.Should().BeOfType<Order>();
            var order = result.AsT0;
            order.Should().BeEquivalentTo(request);
            order.Total.Should().Be(30.0m);
            order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            _repository.Received().Save(order);
        }

        [Fact]
        public void CreateOrder_WithNullCustomerName_ReturnsValidationError()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = null,
                Items = new List<OrderItem>()
            };

            var service = new OrderService(_repository);

            // Act
            var result = service.CreateOrder(request);

            // Assert
            result.Should().BeOfType<ValidationError>();
            var error = result.AsT0;
            error.Message.Should().Be("Customer name is required");
            _repository.DidNotReceive().Save(Arg.Any<Order>());
        }

        [Fact]
        public void CreateOrder_WithEmptyItems_ReturnsValidationError()
        {
            // Arrange
            var request = new CreateOrderRequest
            {
                CustomerName = "John Doe",
                Items = new List<OrderItem>()
            };

            var service = new OrderService(_repository);

            // Act
            var result = service.CreateOrder(request);

            // Assert
            result.Should().BeOfType<ValidationError>();
            var error = result.AsT0;
            error.Message.Should().Be("At least one item is required");
            _repository.DidNotReceive().Save(Arg.Any<Order>());
        }

        [Fact]
        public void CreateOrder_WithExistingOrder_ReturnsOrder()
        {
            // Arrange
            var existingOrder = new Order
            {
                Id = 1,
                CustomerName = "John Doe",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10.0m }
                },
                Total = 20.0m,
                CreatedAt = DateTime.UtcNow
            };

            _repository.GetById(1).Returns(existingOrder);

            var request = new CreateOrderRequest
            {
                CustomerName = "John Doe",
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductName = "Widget", Quantity = 2, UnitPrice = 10.0m }
                }
            };

            var service = new OrderService(_repository);

            // Act
            var result = service.CreateOrder(request);

            // Assert
            result.Should().BeOfType<Order>();
            var order = result.AsT0;
            order.Should().BeEquivalentTo(existingOrder);
            _repository.DidNotReceive().Save(Arg.Any<Order>());
        }
    }

    // Supporting Classes

    public partial class OrderService
    {
        private readonly IOrderRepository _repository;

        public OrderService(IOrderRepository repository)
        {
            _repository = repository;
        }

        public OneOf<Order, ValidationError> CreateOrder(CreateOrderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CustomerName))
            {
                return new ValidationError { Message = "Customer name is required" };
            }

            if (request.Items == null || request.Items.Count == 0)
            {
                return new ValidationError { Message = "At least one item is required" };
            }

            var order = new Order
            {
                CustomerName = request.CustomerName,
                Items = new List<OrderItem>(),
                Total = 0.0m,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var item in request.Items)
            {
                order.Items.Add(item);
                order.Total += item.Quantity * item.UnitPrice;
            }

            _repository.Save(order);
            return order;
        }
    }

    public partial class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public List<OrderItem> Items { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public partial class OrderItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public partial class CreateOrderRequest
    {
        public string CustomerName { get; set; }
        public List<OrderItem> Items { get; set; }
    }

    public partial class ValidationError
    {
        public string Message { get; set; }
    }

    public partial interface IOrderRepository
    {
        void Save(Order order);
        Order? GetById(int id);
    }
}