using OneOf;

// -----------------------------------------------------------------------------
// Domain types
// -----------------------------------------------------------------------------
public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

// -----------------------------------------------------------------------------
// Repository interface
// -----------------------------------------------------------------------------
public interface IProductRepository
{
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

// -----------------------------------------------------------------------------
// Generic command handler interface
// -----------------------------------------------------------------------------
public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command);
}

// -----------------------------------------------------------------------------
// CreateProductCommandHandler implementation
// -----------------------------------------------------------------------------
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
        var added = await _repository.AddAsync(product);
        return added;
    }
}

// -----------------------------------------------------------------------------
// Tests
// -----------------------------------------------------------------------------
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_NameIsNullOrWhitespace_ReturnsValidationError()
    {
        // Arrange
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand(null, 10m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Should().Be(new ValidationError("Name is required"));

        // Verify no repository calls
        await repo.DidNotReceive().ExistsAsync(Arg.Any<string>());
        await repo.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_PriceIsZeroOrNegative_ReturnsValidationError()
    {
        // Arrange
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand("Widget", 0m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT1.Should().BeTrue();
        result.AsT1.Should().Be(new ValidationError("Price must be positive"));

        // Verify no repository calls
        await repo.DidNotReceive().ExistsAsync(Arg.Any<string>());
        await repo.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_ProductAlreadyExists_ReturnsDuplicateError()
    {
        // Arrange
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Widget").Returns(Task.FromResult(true));
        var handler = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand("Widget", 10m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT2.Should().BeTrue();
        result.AsT2.Should().Be(new DuplicateError("Widget"));

        // Verify repository calls
        await repo.Received(1).ExistsAsync("Widget");
        await repo.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_AddsProductAndReturnsIt()
    {
        // Arrange
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Widget").Returns(Task.FromResult(false));
        var addedProduct = new Product(1, "Widget", 10m);
        repo.AddAsync(Arg.Is<Product>(p => p.Id == 0 && p.Name == "Widget" && p.Price == 10m))
            .Returns(Task.FromResult(addedProduct));

        var handler = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand("Widget", 10m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().Be(addedProduct);

        // Verify repository calls
        await repo.Received(1).ExistsAsync("Widget");
        await repo.Received(1).AddAsync(Arg.Is<Product>(p => p.Id == 0 && p.Name == "Widget" && p.Price == 10m));
    }
}