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
            return OneOf<Product, ValidationError, DuplicateError>.FromT1(new ValidationError("Name is required"));
        }

        if (command.Price <= 0)
        {
            return OneOf<Product, ValidationError, DuplicateError>.FromT1(new ValidationError("Price must be positive"));
        }

        if (await _repository.ExistsAsync(command.Name))
        {
            return OneOf<Product, ValidationError, DuplicateError>.FromT2(new DuplicateError(command.Name));
        }

        var product = await _repository.AddAsync(new Product(0, command.Name, command.Price));
        return OneOf<Product, ValidationError, DuplicateError>.FromT0(product);
    }
}

// --- Unit Tests ---

public class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(_repository);
    }

    public async Task HandleAsync_ShouldReturnValidationError_WhenNameIsEmpty()
    {
        var command = new CreateProductCommand("", 10.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Be("Name is required");
    }

    public async Task HandleAsync_ShouldReturnValidationError_WhenPriceIsInvalid()
    {
        var command = new CreateProductCommand("Valid Name", -1m);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Be("Price must be positive");
    }

    public async Task HandleAsync_ShouldReturnDuplicateError_WhenProductExists()
    {
        var command = new CreateProductCommand("Existing", 10m);
        _repository.ExistsAsync(command.Name).Returns(true);

        var result = await _handler.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        var error = result.AsT2;
        error.ExistingName.Should().Be("Existing");
    }

    public async Task HandleAsync_ShouldReturnProduct_WhenInputIsValid()
    {
        var command = new CreateProductCommand("New Product", 50m);
        var expectedProduct = new Product(1, "New Product", 50m);

        _repository.ExistsAsync(command.Name).Returns(false);
        _repository.AddAsync(Arg.Any<Product>()).Returns(expectedProduct);

        var result = await _handler.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeEquivalentTo(expectedProduct);

        await _repository.Received(1).AddAsync(Arg.Any<Product>());
    }
}