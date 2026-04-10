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

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists)
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
    public async Task HandleAsync_WhenNameIsNullOrEmpty_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand(string.Empty, 10.0m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_WhenPriceIsZero_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test", 0.0m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_WhenNameExists_ReturnsDuplicateError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("Existing").Returns(true);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Existing", 10.0m);

        var result = await handler.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        var error = result.AsT2;
        error.ExistingName.Should().Be("Existing");
    }

    [Fact]
    public async Task HandleAsync_WhenValidCommand_ReturnsProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("New").Returns(false);
        repository.AddAsync(Arg.Any<Product>()).Returns(Task.FromResult(new Product(1, "New", 10.0m)));
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("New", 10.0m);

        var result = await handler.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        var product = result.AsT0;
        product.Name.Should().Be("New");
        product.Price.Should().Be(10.0m);
    }
}