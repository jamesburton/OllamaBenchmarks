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

        // Check for duplicates
        if (await _repository.ExistsAsync(command.Name))
        {
            return new DuplicateError(command.Name);
        }

        // Success path
        var productToSave = new Product(0, command.Name, command.Price);
        var product = await _repository.AddAsync(productToSave);
        return product;
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_NameIsMissing_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand(null, 10.00m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        // Check if the result is a ValidationError (T1)
        result.Should().IsT1();
        var validationError = result.AsT1();
        validationError.Should().BeOfType<ValidationError>();
        validationError.Should().Be(new ValidationError("Name is required"));
    }

    [Fact]
    public async Task HandleAsync_PriceIsZero_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test", 0m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().IsT1();
        var validationError = result.AsT1();
        validationError.Should().BeOfType<ValidationError>();
        validationError.Should().Be(new ValidationError("Price must be positive"));
    }

    [Fact]
    public async Task HandleAsync_ProductAlreadyExists_ReturnsDuplicateError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        // Setup repository to report existence
        repository.ExistsAsync("ExistingProduct").ReturnsAsync(true);

        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("ExistingProduct", 50.00m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        // Check if the result is a DuplicateError (T2)
        result.Should().IsT2();
        var duplicateError = result.AsT2();
        duplicateError.Should().BeOfType<DuplicateError>();
        duplicateError.Should().Be(new DuplicateError("ExistingProduct"));

        // Ensure AddAsync was never called
        await repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_Success_AddsProductAndReturnsProduct()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        // Setup repository to report non-existence
        repository.ExistsAsync("NewProduct").ReturnsAsync(false);

        // Setup AddAsync to return a successful product
        var expectedProduct = new Product(101, "NewProduct", 99.99m);
        repository.AddAsync(Arg.Any<Product>()).Returns(expectedProduct);

        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("NewProduct", 99.99m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        // Check if the result is a Product (T0)
        result.Should().IsT0();
        var product = result.AsT0();
        product.Should().BeOfType<Product>();
        product.Should().BeEquivalentTo(expectedProduct);

        // Verify that AddAsync was called exactly once with the correct product data
        await repository.Received(1).AddAsync(
            Arg.Is<Product>(p => p.Name == "NewProduct" && p.Price == 99.99m)
        );
    }
}