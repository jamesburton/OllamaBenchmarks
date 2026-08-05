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
    private readonly IProductRepository _repository;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _handler = new CreateProductCommandHandler(_repository);
    }

    [Fact]
    public async Task HandleAsync_WithNullName_ReturnsValidationError()
    {
        var command = new CreateProductCommand(null, 10.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_WithNegativePrice_ReturnsValidationError()
    {
        var command = new CreateProductCommand("Widget", -5.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_WhenExists_ReturnsDuplicateError()
    {
        _repository.ExistsAsync("Widget").Returns(Task.FromResult(true));
        var command = new CreateProductCommand("Widget", 10.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT2.Should().Be(true);
        result.AsT2.ExistingName.Should().Be("Widget");
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ReturnsProduct()
    {
        var expectedProduct = new Product(1, "Widget", 10.0m);
        _repository.ExistsAsync("Widget").Returns(Task.FromResult(false));
        _repository.AddAsync(Arg.Any<Product>()).Returns(Task.FromResult(expectedProduct));

        var command = new CreateProductCommand("Widget", 10.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT0.Should().Be(true);
        result.AsT0.Should().Be(expectedProduct);
    }
}