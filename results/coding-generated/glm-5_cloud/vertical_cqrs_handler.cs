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
    public async Task HandleAsync_ReturnsValidationError_WhenNameIsNullOrWhitespace()
    {
        var command = new CreateProductCommand(null, 100);

        var result = await _sut.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Should().Be(new ValidationError("Name is required"));
    }

    [Fact]
    public async Task HandleAsync_ReturnsValidationError_WhenPriceIsNotPositive()
    {
        var command = new CreateProductCommand("ValidName", 0);

        var result = await _sut.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Should().Be(new ValidationError("Price must be positive"));
    }

    [Fact]
    public async Task HandleAsync_ReturnsDuplicateError_WhenProductExists()
    {
        var command = new CreateProductCommand("Existing", 100);
        _repository.ExistsAsync("Existing").Returns(true);

        var result = await _sut.HandleAsync(command);

        result.IsT2.Should().Be(true);
        result.AsT2.Should().Be(new DuplicateError("Existing"));
    }

    [Fact]
    public async Task HandleAsync_ReturnsProduct_WhenCreationSuccessful()
    {
        var command = new CreateProductCommand("NewProduct", 100);
        var expected = new Product(1, "NewProduct", 100);
        _repository.ExistsAsync("NewProduct").Returns(false);
        _repository.AddAsync(Arg.Any<Product>()).Returns(expected);

        var result = await _sut.HandleAsync(command);

        result.IsT0.Should().Be(true);
        result.AsT0.Should().Be(expected);
    }
}