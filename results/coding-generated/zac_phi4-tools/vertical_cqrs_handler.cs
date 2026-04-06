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

        var product = await _repository.AddAsync(new Product(0, command.Name, command.Price));
        return product;
    }
}

// Tests
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenNameIsNullOrWhitespace()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        // Act
        var result = await handler.HandleAsync(new CreateProductCommand("", 10));

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenPriceIsNotPositive()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        // Act
        var result = await handler.HandleAsync(new CreateProductCommand("Product", -5));

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnDuplicateError_WhenProductNameExists()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(true);
        var handler = new CreateProductCommandHandler(repository);

        // Act
        var result = await handler.HandleAsync(new CreateProductCommand("Existing", 10));

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnProduct_WhenAllValid()
    {
        // Arrange
        var product = new Product(1, "New", 10);
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        repository.AddAsync(Arg.Is<Product>(p => p.Name == "New")).Returns(product);
        var handler = new CreateProductCommandHandler(repository);

        // Act
        var result = await handler.HandleAsync(new CreateProductCommand("New", 10));

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeEquivalentTo(product);
    }
}