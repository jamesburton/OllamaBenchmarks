using OneOf;
using NSubstitute;
using AwesomeAssertions;

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
        // Validation 1: Name check
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new ValidationError("Name is required");
        }

        // Validation 2: Price check
        if (command.Price <= 0)
        {
            return new ValidationError("Price must be positive");
        }

        // Check for duplicates
        bool exists = await _repository.ExistsAsync(command.Name);
        if (exists)
        {
            return new DuplicateError(command.Name);
        }

        // Add product
        var newProduct = new Product(0, command.Name, command.Price);
        await _repository.AddAsync(newProduct);

        // Return the created product
        return newProduct;
    }
}

// Example Test Setup (Demonstration of usage based on constraints)
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsProduct()
    {
        // Arrange
        var mockRepository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand("Test Product", 10.99m);

        // Setup repository to report no existence
        mockRepository.ExistsAsync("Test Product").Returns(Task.FromResult(false));
        mockRepository.AddAsync(Arg.Any<Product>()).Returns(Task.CompletedTask);

        var handler = new CreateProductCommandHandler(mockRepository);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<Product>();
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().NotBeNull();
        result.AsT0.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsDuplicateError()
    {
        // Arrange
        var mockRepository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand("Duplicate", 5.00m);

        // Setup repository to report existence
        mockRepository.ExistsAsync("Duplicate").Returns(Task.FromResult(true));

        var handler = new CreateProductCommandHandler(mockRepository);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<DuplicateError>();
        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Duplicate");
    }

    [Fact]
    public async Task HandleAsync_NullName_ReturnsValidationError()
    {
        // Arrange
        var mockRepository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand(null, 10.00m);

        var handler = new CreateProductCommandHandler(mockRepository);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<ValidationError>();
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_NegativePrice_ReturnsValidationError()
    {
        // Arrange
        var mockRepository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand("Test", -5.00m);

        var handler = new CreateProductCommandHandler(mockRepository);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<ValidationError>();
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }
}