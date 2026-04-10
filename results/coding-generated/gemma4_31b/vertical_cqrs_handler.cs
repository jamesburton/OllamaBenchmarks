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

public class CreateProductCommandHandlerTests
{
    [Xunit.Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenNameIsEmpty()
    {
        // Arrange
        var repo = Substitute.For<IProductRepository>();
        var sut = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand("", 10m);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Xunit.Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenPriceIsZeroOrLess()
    {
        // Arrange
        var repo = Substitute.For<IProductRepository>();
        var sut = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand("Gadget", 0m);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Xunit.Fact]
    public async Task HandleAsync_ShouldReturnDuplicateError_WhenProductExists()
    {
        // Arrange
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Existing").Returns(true);
        var sut = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand("Existing", 10m);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing");
    }

    [Xunit.Fact]
    public async Task HandleAsync_ShouldReturnProduct_WhenInputIsValid()
    {
        // Arrange
        var repo = Substitute.For<IProductRepository>();
        var expectedProduct = new Product(1, "New Product", 10m);

        repo.ExistsAsync("New Product").Returns(false);
        repo.AddAsync(Arg.Any<Product>()).Returns(expectedProduct);

        var sut = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand("New Product", 10m);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.Id.Should().Be(1);
        result.AsT0.Name.Should().Be("New Product");
    }
}