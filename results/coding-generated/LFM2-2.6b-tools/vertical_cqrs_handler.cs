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
    private readonly IProductRepository repository;

    public CreateProductCommandHandler(IProductRepository repository) => this.repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return new ValidationError("Name is required");

        if (command.Price <= 0)
            return new ValidationError("Price must be positive");

        var existing = await repository.ExistsAsync(command.Name);
        if (existing)
            return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await repository.AddAsync(product);
        return result;
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_NameNull_ReturnsValidationError_WithNameRequiredMessage()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        var command = new CreateProductCommand(string.Empty, 10m);

        var result = await handler.HandleAsync(command);

        result.Should().BeOfType<ValidationError>();
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeEquivalentTo(new ValidationError("Name is required"));
    }

    [Fact]
    public async Task HandleAsync_NameWhitespace_ReturnsValidationError_WithNameRequiredMessage()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        var command = new CreateProductCommand("   ", 10m);

        var result = await handler.HandleAsync(command);

        result.Should().BeOfType<ValidationError>();
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeEquivalentTo(new ValidationError("Name is required"));
    }

    [Fact]
    public async Task HandleAsync_PriceNonPositive_ReturnsValidationError_WithPriceMustBePositiveMessage()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        var command = new CreateProductCommand("Product", -5m);

        var result = await handler.HandleAsync(command);

        result.Should().BeOfType<ValidationError>();
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeEquivalentTo(new ValidationError("Price must be positive"));
    }

    [Fact]
    public async Task HandleAsync_NameExists_ReturnsDuplicateError_WithExistingName()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.Setup(r => r.ExistsAsync("ExistingProduct")).ReturnsAsync(true);
        var handler = new CreateProductCommandHandler(repository);

        var command = new CreateProductCommand("ExistingProduct", 10m);

        var result = await handler.HandleAsync(command);

        result.Should().BeOfType<DuplicateError>();
        result.IsT1.Should().BeTrue();
        result.AsT1.Should().BeEquivalentTo(new DuplicateError("ExistingProduct"));
    }

    [Fact]
    public async Task HandleAsync_NameUnique_AddsProductAndReturnsProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.Setup(r => r.ExistsAsync("NewProduct")).ReturnsAsync(false);
        repository.Setup(r => r.AddAsync(It.IsAny<Product>())).ReturnsAsync(new Product(1, "NewProduct", 10m));
        var handler = new CreateProductCommandHandler(repository);

        var command = new CreateProductCommand("NewProduct", 10m);

        var result = await handler.HandleAsync(command);

        result.Should().BeOfType<Product>();
        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeEquivalentTo(new Product(1, "NewProduct", 10m));
    }
}