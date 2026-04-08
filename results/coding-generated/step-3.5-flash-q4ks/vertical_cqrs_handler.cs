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
        var addedProduct = await _repository.AddAsync(product);
        return addedProduct;
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenNameIsNullOrWhiteSpace_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("", 10);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_WhenPriceIsZeroOrNegative_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Product", 0);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_WhenProductNameExists_ReturnsDuplicateError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("Existing").Returns(true);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Existing", 10);

        var result = await handler.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing");
    }

    [Fact]
    public async Task HandleAsync_WhenProductIsValid_ReturnsProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("NewProduct").Returns(false);
        var expectedProduct = new Product(1, "NewProduct", 10);
        repository.AddAsync(Arg.Any<Product>()).Returns(expectedProduct);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("NewProduct", 10);

        var result = await handler.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().Be(expectedProduct);
    }
}