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
    private readonly IProductRepository _repository;
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _handler = new CreateProductCommandHandler(_repository);
    }

    public async Task HandleAsync_WhenNameIsNull_ReturnsValidationError()
    {
        var command = new CreateProductCommand(null, 10.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Name is required");
    }

    public async Task HandleAsync_WhenNameIsEmpty_ReturnsValidationError()
    {
        var command = new CreateProductCommand("", 10.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Name is required");
    }

    public async Task HandleAsync_WhenNameIsWhitespace_ReturnsValidationError()
    {
        var command = new CreateProductCommand("   ", 10.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Name is required");
    }

    public async Task HandleAsync_WhenPriceIsZero_ReturnsValidationError()
    {
        var command = new CreateProductCommand("Widget", 0);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    public async Task HandleAsync_WhenPriceIsNegative_ReturnsValidationError()
    {
        var command = new CreateProductCommand("Widget", -5.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    public async Task HandleAsync_WhenProductExists_ReturnsDuplicateError()
    {
        _repository.ExistsAsync("Widget").Returns(Task.FromResult(true));
        var command = new CreateProductCommand("Widget", 10.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT2.Should().Be(true);
        result.AsT2.ExistingName.Should().Be("Widget");
    }

    public async Task HandleAsync_WhenProductDoesNotExist_ReturnsProduct()
    {
        _repository.ExistsAsync("Widget").Returns(Task.FromResult(false));
        var expectedProduct = new Product(123, "Widget", 10.0m);
        _repository.AddAsync(Arg.Any<Product>()).Returns(Task.FromResult(expectedProduct));

        var command = new CreateProductCommand("Widget", 10.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT0.Should().Be(true);
        result.AsT0.Id.Should().Be(123);
        result.AsT0.Name.Should().Be("Widget");
        result.AsT0.Price.Should().Be(10.0m);
    }

    public async Task HandleAsync_WhenProductDoesNotExist_AddAsyncCalledWithCorrectProduct()
    {
        _repository.ExistsAsync("Widget").Returns(Task.FromResult(false));
        var expectedProduct = new Product(123, "Widget", 10.0m);
        _repository.AddAsync(Arg.Any<Product>()).Returns(Task.FromResult(expectedProduct));

        var command = new CreateProductCommand("Widget", 10.0m);
        await _handler.HandleAsync(command);

        await _repository.Received(1).AddAsync(Arg.Is<Product>(p =>
            p.Name == "Widget" && p.Price == 10.0m));
    }
}