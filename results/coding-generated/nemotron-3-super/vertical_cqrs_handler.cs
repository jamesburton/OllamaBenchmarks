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

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_NameIsNullOrWhitespace_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        var result = await handler.HandleAsync(new CreateProductCommand(null, 10m));

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_PriceIsZeroOrNegative_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        var result = await handler.HandleAsync(new CreateProductCommand("Test", 0m));

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_NameExists_ReturnsDuplicateError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(true);
        var handler = new CreateProductCommandHandler(repository);

        var result = await handler.HandleAsync(new CreateProductCommand("Existing", 10m));

        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing");
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        var expectedProduct = new Product(1, "NewProduct", 15.99m);
        repository.AddAsync(Arg.Any<Product>()).Returns(expectedProduct);
        var handler = new CreateProductCommandHandler(repository);

        var result = await handler.HandleAsync(new CreateProductCommand("NewProduct", 15.99m));

        result.IsT0.Should().BeTrue();
        var product = result.AsT0;
        product.Id.Should().Be(1);
        product.Name.Should().Be("NewProduct");
        product.Price.Should().Be(15.99m);
    }
}