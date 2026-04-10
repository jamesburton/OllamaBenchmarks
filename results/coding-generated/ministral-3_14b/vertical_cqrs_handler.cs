using OneOf;

public record CreateProductCommand(string Name, decimal Price);

public record Product(int Id, string Name, decimal Price);

public record ValidationError(string Message);

public record DuplicateError(string ExistingName);

public interface IProductRepository
{
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command);
}

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new ValidationError("Name is required");
        }

        if (command.Price <= 0)
        {
            return new ValidationError("Price must be positive");
        }

        if (await _repository.ExistsAsync(command.Name))
        {
            return new DuplicateError(command.Name);
        }

        var product = await _repository.AddAsync(new Product(0, command.Name, command.Price));
        return product;
    }
}

// Test class
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithInvalidName_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand(null, 10);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<OneOf<Product, ValidationError, DuplicateError>>();
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidPrice_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test", 0);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<OneOf<Product, ValidationError, DuplicateError>>();
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateName_ReturnsDuplicateError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(true);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Duplicate", 10);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<OneOf<Product, ValidationError, DuplicateError>>();
        result.IsT2.Should().BeTrue();
        result.AsT2.Should().Be("Duplicate");
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsProduct()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        repository.AddAsync(Arg.Any<Product>()).Returns(new Product(1, "Valid", 10));
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Valid", 10);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<OneOf<Product, ValidationError, DuplicateError>>();
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeEquivalentTo(new Product(1, "Valid", 10));
    }
}