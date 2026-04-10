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

        var product = new Product(0, command.Name, command.Price);
        return await _repository.AddAsync(product);
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test Product", 10.99m);
        var expectedProduct = new Product(1, "Test Product", 10.99m);

        repository.ExistsAsync(command.Name).Returns(false);
        repository.AddAsync(Arg.Is<Product>(p => p.Name == command.Name && p.Price == command.Price))
                  .Returns(expectedProduct);

        var result = await handler.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().Be(expectedProduct);
        await repository.Received(1).ExistsAsync(command.Name);
        await repository.Received(1).AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_EmptyName_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("", 10.99m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
        await repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
        await repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_WhitespaceName_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("   ", 10.99m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
        await repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
        await repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_ZeroPrice_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test Product", 0m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
        await repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
        await repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_NegativePrice_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Test Product", -5m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
        await repository.DidNotReceive().ExistsAsync(Arg.Any<string>());
        await repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsDuplicateError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Existing Product", 10.99m);

        repository.ExistsAsync(command.Name).Returns(true);

        var result = await handler.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing Product");
        await repository.Received(1).ExistsAsync(command.Name);
        await repository.DidNotReceive().AddAsync(Arg.Any<Product>());
    }
}