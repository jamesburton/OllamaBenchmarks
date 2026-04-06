using OneOf;
using NSubstitute;
using Xunit;
using System.Threading.Tasks;
using System;

// 1. Records and types
public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

// 2. IProductRepository interface
public interface IProductRepository
{
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

// 3. ICommandHandler interface
public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command);
}

// 4. CreateProductCommandHandler implementation
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
    {
        // Validation 1: Name required
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new ValidationError("Name is required");
        }

        // Validation 2: Price positive
        if (command.Price <= 0)
        {
            return new ValidationError("Price must be positive");
        }

        // Validation 3: Duplicate check
        if (await _repository.ExistsAsync(command.Name))
        {
            return new DuplicateError(command.Name);
        }

        // Success path
        var newProduct = new Product(0, command.Name, command.Price);
        var product = await _repository.AddAsync(newProduct);
        return product;
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsProduct()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand("Laptop", 1200.00m);

        // Setup mock behavior: Does not exist, and returns a product on add
        mockRepo.ExistsAsync(command.Name).Returns(Task.FromResult(false));
        mockRepo.AddAsync(Arg.Any<Product>()).Returns(new Product(1, "Laptop", 1200.00m));

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        // Check if the result is a Product (T0)
        result.Should().IsT0.Should().NotBeNull();
        var product = result.AsT0;
        product.Should().BeEquivalentTo(new Product(1, "Laptop", 1200.00m));

        // Verify repository calls
        await mockRepo.Received(1).ExistsAsync(command.Name);
        await mockRepo.Received(1).AddAsync(Arg.Is<Product>(p => p.Name == "Laptop" && p.Price == 1200.00m));
    }

    [Fact]
    public async Task HandleAsync_NullName_ReturnsValidationError()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand(null, 10.00m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        // Check if the result is a ValidationError (T1)
        result.Should().IsT1.Should().NotBeNull();
        var error = result.AsT1;
        error.Should().BeEquivalentTo(new ValidationError("Name is required"));

        // Ensure repository was never called
        await mockRepo.DidNotReceive().ExistsAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_WhitespaceName_ReturnsValidationError()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand("   ", 10.00m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().IsT1.Should().NotBeNull();
        var error = result.AsT1;
        error.Should().BeEquivalentTo(new ValidationError("Name is required"));
    }

    [Fact]
    public async Task HandleAsync_ZeroPrice_ReturnsValidationError()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand("Test", 0m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().IsT1.Should().NotBeNull();
        var error = result.AsT1;
        error.Should().BeEquivalentTo(new ValidationError("Price must be positive"));
    }

    [Fact]
    public async Task HandleAsync_NegativePrice_ReturnsValidationError()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand("Test", -5m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().IsT1.Should().NotBeNull();
        var error = result.AsT1;
        error.Should().BeEquivalentTo(new ValidationError("Price must be positive"));
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsDuplicateError()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand("Widget", 5.00m);

        // Setup mock behavior: Exists
        mockRepo.ExistsAsync(command.Name).Returns(Task.FromResult(true));

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        // Check if the result is a DuplicateError (T2)
        result.Should().IsT2.Should().NotBeNull();
        var error = result.AsT2;
        error.Should().BeEquivalentTo(new DuplicateError("Widget"));

        // Ensure AddAsync was never called
        await mockRepo.DidNotReceive().AddAsync(Arg.Any<Product>());
    }
}