using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository
{
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>>
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

// Tests
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenNameIsNull()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand(null, 10m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenPriceIsNonPositive()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test Product", 0m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnDuplicateError_WhenProductAlreadyExists()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(true);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Existing Product", 10m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing Product");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnProduct_WhenValidCommand()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        repository.AddAsync(Arg.Any<Product>()).Returns(new Product(1, "New Product", 10m));
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("New Product", 10m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.Name.Should().Be("New Product");
    }
}