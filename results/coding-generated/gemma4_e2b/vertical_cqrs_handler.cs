using OneOf;
using NSubstitute;
using AwesomeAssertions;

// 1. Records and Types
public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

// 2. IProductRepository Interface
public interface IProductRepository
{
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

// 3. Generic Command Handler Interface
public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command);
}

// 4. CreateProductCommandHandler Implementation
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
    {
        // 1. Validation: Name check
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new ValidationError("Name is required");
        }

        // 2. Validation: Price check
        if (command.Price <= 0)
        {
            return new ValidationError("Price must be positive");
        }

        // 3. Duplicate Check
        bool exists = await _repository.ExistsAsync(command.Name);
        if (exists)
        {
            return new DuplicateError(command.Name);
        }

        // 4. Success: Add Product
        var newProduct = new Product(0, command.Name, command.Price);
        await _repository.AddAsync(newProduct);
        return newProduct;
    }
}

// Example Test Class
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidProduct_ReturnsProduct()
    {
        // Arrange
        var mockRepository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepository);
        var command = new CreateProductCommand("Laptop", 1200.50m);

        // Setup: Product does not exist
        mockRepository.ExistsAsync("Laptop").Returns(Task.FromResult(false));
        // Setup: AddAsync returns a new product (Id 0 is used in the handler logic)
        mockRepository.AddAsync(Arg.Any<Product>()).Returns(Task.CompletedTask);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<Product>();
        result.AsT0.Should().NotBeNull();
        result.AsT0.Name.Should().Be("Laptop");
        result.AsT0.Price.Should().Be(1200.50m);

        // Verify interaction
        mockRepository.ExistsAsync("Laptop").Received(1);
        mockRepository.AddAsync(Arg.Is<Product>(p => p.Name == "Laptop")).Received(1);
    }

    [Fact]
    public async Task HandleAsync_NameMissing_ReturnsValidationError()
    {
        // Arrange
        var mockRepository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepository);
        var command = new CreateProductCommand(null, 10.0m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<ValidationError>();
        result.AsT0.Should().Be("Name is required");

        // Verify no repository calls were made
        mockRepository.ExistsAsync(Arg.Any<string>()).DidNotReceive();
        mockRepository.AddAsync(Arg.Any<Product>()).DidNotReceive();
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsDuplicateError()
    {
        // Arrange
        var mockRepository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepository);
        var command = new CreateProductCommand("ExistingItem", 50.0m);

        // Setup: Product exists
        mockRepository.ExistsAsync("ExistingItem").Returns(Task.FromResult(true));

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<DuplicateError>();
        result.AsT0.Should().Be("ExistingItem");

        // Verify ExistsAsync was called
        mockRepository.ExistsAsync("ExistingItem").Received(1);
        // Verify AddAsync was NOT called
        mockRepository.AddAsync(Arg.Any<Product>()).DidNotReceive();
    }

    [Fact]
    public async Task HandleAsync_NegativePrice_ReturnsValidationError()
    {
        // Arrange
        var mockRepository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepository);
        var command = new CreateProductCommand("Test", -10.0m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<ValidationError>();
        result.AsT0.Should().Be("Price must be positive");

        // Verify no repository calls were made
        mockRepository.ExistsAsync(Arg.Any<string>()).DidNotReceive();
        mockRepository.AddAsync(Arg.Any<Product>()).DidNotReceive();
    }
}