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
    private readonly IProductRepository _repository;
    private readonly CreateProductCommandHandler _sut;

    public CreateProductCommandHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _sut = new CreateProductCommandHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenNameIsNull()
    {
        var command = new CreateProductCommand(null, 100m);

        var result = await _sut.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenNameIsWhitespace()
    {
        var command = new CreateProductCommand("   ", 100m);

        var result = await _sut.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenPriceIsZeroOrNegative()
    {
        var command = new CreateProductCommand("Valid", 0m);

        var result = await _sut.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnDuplicateError_WhenProductExists()
    {
        var command = new CreateProductCommand("Existing", 10m);
        _repository.ExistsAsync("Existing").Returns(true);

        var result = await _sut.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnProduct_WhenValid()
    {
        var command = new CreateProductCommand("New", 10m);
        var expectedProduct = new Product(1, "New", 10m);

        _repository.ExistsAsync("New").Returns(false);
        _repository.AddAsync(Arg.Any<Product>()).Returns(expectedProduct);

        var result = await _sut.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().Be(expectedProduct);
    }
}