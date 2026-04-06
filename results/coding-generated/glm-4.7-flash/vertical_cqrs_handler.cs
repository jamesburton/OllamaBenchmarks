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

        var product = new Product(0, command.Name, command.Price);
        return await _repository.AddAsync(product);
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_ReturnsProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand("Test", 10.0m);
        var expectedProduct = new Product(1, "Test", 10.0m);
        repository.ExistsAsync("Test").Returns(false);
        repository.AddAsync(Arg.Any<Product>()).Returns(expectedProduct);

        var handler = new CreateProductCommandHandler(repository);

        var result = await handler.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeEquivalentTo(expectedProduct);
    }

    [Fact]
    public async Task HandleAsync_WithWhitespaceName_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand("   ", 10.0m);

        var handler = new CreateProductCommandHandler(repository);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_WithNegativePrice_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand("Test", -5.0m);

        var handler = new CreateProductCommandHandler(repository);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateName_ReturnsDuplicateError()
    {
        var repository = Substitute.For<IProductRepository>();
        var command = new CreateProductCommand("Existing", 10.0m);
        repository.ExistsAsync("Existing").Returns(true);

        var handler = new CreateProductCommandHandler(repository);

        var result = await handler.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing");
    }
}