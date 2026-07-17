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

        return await _repository.AddAsync(new Product(0, command.Name, command.Price));
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenNameIsNull_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand(null, 10m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Name is required");
        await repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
        await repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsEmpty_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand(string.Empty, 10m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_WhenNameIsWhitespace_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("   ", 10m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_WhenPriceIsZero_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Widget", 0m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Price must be positive");
        await repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
        await repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_WhenPriceIsNegative_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Widget", -1m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().Be(true);
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_WhenNameAlreadyExists_ReturnsDuplicateError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("Existing").Returns(true);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Existing", 25m);

        var result = await handler.HandleAsync(command);

        result.IsT2.Should().Be(true);
        result.AsT2.ExistingName.Should().Be("Existing");
        await repository.Received(1).ExistsAsync("Existing");
        await repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_WhenCommandIsValid_ReturnsCreatedProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("NewProduct").Returns(false);
        repository.AddAsync(Arg.Any<Product>()).Returns(new Product(42, "NewProduct", 19.99m));
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("NewProduct", 19.99m);

        var result = await handler.HandleAsync(command);

        result.IsT0.Should().Be(true);
        result.AsT0.Id.Should().Be(42);
        result.AsT0.Name.Should().Be("NewProduct");
        result.AsT0.Price.Should().Be(19.99m);
        await repository.Received(1).AddAsync(Arg.Is<Product>(p => p.Id == 0 && p.Name == "NewProduct" && p.Price == 19.99m));
    }
}