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
            return new ValidationError("Name is required");

        if (command.Price <= 0)
            return new ValidationError("Price must be positive");

        if (await _repository.ExistsAsync(command.Name))
            return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        return await _repository.AddAsync(product);
    }
}

// Tests using NSubstitute and AwesomeAssertions
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsProduct()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("Test Product").Returns(false);
        repository.AddAsync(Arg.Any<Product>()).Returns(new Product(1, "Test Product", 10.0m));

        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test Product", 10.0m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Should().BeOfType<Product>();
        result.AsT2.Id.Should().Be(1);
        result.AsT2.Name.Should().Be("Test Product");
        result.AsT2.Price.Should().Be(10.0m);
    }

    [Fact]
    public async Task HandleAsync_NameIsEmpty_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("", 10.0m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Should().BeOfType<ValidationError>();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_PriceIsNegative_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test Product", -5.0m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Should().BeOfType<ValidationError>();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsDuplicateError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("Test Product").Returns(true);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test Product", 10.0m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Should().BeOfType<DuplicateError>();
        result.AsT2.Message.Should().Be("Test Product");
    }
}