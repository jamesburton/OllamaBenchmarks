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
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly CreateProductCommandHandler _sut;

    public CreateProductCommandHandlerTests()
    {
        _sut = new CreateProductCommandHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenNameIsNull()
    {
        var command = new CreateProductCommand(null, 100m);

        var result = await _sut.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Should().Be(new ValidationError("Name is required"));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenNameIsWhitespace()
    {
        var command = new CreateProductCommand("   ", 100m);

        var result = await _sut.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Should().Be(new ValidationError("Name is required"));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenPriceIsNotPositive()
    {
        var command = new CreateProductCommand("ValidName", 0m);

        var result = await _sut.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Should().Be(new ValidationError("Price must be positive"));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnDuplicateError_WhenProductExists()
    {
        var command = new CreateProductCommand("ExistingProduct", 50m);
        _repository.ExistsAsync("ExistingProduct").Returns(true);

        var result = await _sut.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        result.AsT2.Should().Be(new DuplicateError("ExistingProduct"));
        await _repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnProduct_WhenCreationIsValid()
    {
        var command = new CreateProductCommand("NewProduct", 50m);
        var expectedProduct = new Product(1, "NewProduct", 50m);
        _repository.ExistsAsync("NewProduct").Returns(false);
        _repository.AddAsync(Arg.Any<Product>()).Returns(expectedProduct);

        var result = await _sut.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().Be(expectedProduct);
    }
}