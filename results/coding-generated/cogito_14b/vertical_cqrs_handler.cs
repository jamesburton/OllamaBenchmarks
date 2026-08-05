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

        if (command.Price <= 0m)
            return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists)
            return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        return await _repository.AddAsync(product);
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithNullName_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        // Act
        var result = await handler.HandleAsync(new CreateProductCommand(null, 10m));

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_WithNegativePrice_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        // Act
        var result = await handler.HandleAsync(new CreateProductCommand("Test", -1m));

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_WithExistingName_ReturnsDuplicateError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(true);
        var handler = new CreateProductCommandHandler(repository);

        // Act
        var result = await handler.HandleAsync(new CreateProductCommand("Test", 10m));

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Test");
    }

    [Fact]
    public async Task HandleAsync_WithValidData_ReturnsNewProduct()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        repository.AddAsync(Arg.Any<Product>()).Returns(new Product(1, "Test", 10m));
        var handler = new CreateProductCommandHandler(repository);

        // Act
        var result = await handler.HandleAsync(new CreateProductCommand("Test", 10m));

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.Name.Should().Be("Test");
        result.AsT0.Price.Should().Be(10m);
    }
}