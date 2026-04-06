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
        var result = await _repository.AddAsync(product);
        return result;
    }
}

// Test class
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsProduct()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Test").Returns(false);
        repo.AddAsync(Arg.Any<Product>()).Returns(new Product(1, "Test", 10m));

        var handler = new CreateProductCommandHandler(repo);

        var command = new CreateProductCommand("Test", 10m);
        var result = await handler.HandleAsync(command);

        result.Should().BeOfType<Product>();
        var product = result.AsT0;
        product.Name.Should().Be("Test");
        product.Price.Should().Be(10m);
    }

    [Fact]
    public async Task HandleAsync_NameIsEmpty_ReturnsValidationError()
    {
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);

        var command = new CreateProductCommand("", 10m);
        var result = await handler.HandleAsync(command);

        result.Should().BeOfType<ValidationError>();
        var err = result.AsT1; // Wait: ordering: OneOf<Product, ValidationError, DuplicateError> => T0=Product, T1=ValidationError, T2=DuplicateError
        // Actually AsT1 is ValidationError, AsT2 is DuplicateError.
        // But we need to use .AsT1.
        // Use .IsT1 to check.
        result.IsT1.Should().BeTrue();
        var error = result.AsT1;
        error.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsDuplicateError()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Existing").Returns(true);
        var handler = new CreateProductCommandHandler(repo);

        var command = new CreateProductCommand("Existing", 5m);
        var result = await handler.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        var dup = result.AsT2;
        dup.ExistingName.Should().Be("Existing");
    }

    [Fact]
    public async Task HandleAsync_NegativePrice_ReturnsValidationError()
    {
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);

        var command = new CreateProductCommand("New", -1m);
        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        var err = result.AsT1;
        err.Message.Should().Be("Price must be positive");
    }
}

result.IsT0.Should().BeTrue();
var product = result.AsT0;

result.IsT0.Should().BeTrue();
var product = result.AsT0;
product.Id.Should().Be(1);
product.Name.Should().Be("Test");
product.Price.Should().Be(10m);

result.IsT1.Should().BeTrue();
var err = result.AsT1;
err.Message.Should().Be("Name is required");

result.IsT2.Should().BeTrue();
var dup = result.AsT2;
dup.ExistingName.Should().Be("Existing");

result.IsT1.Should().BeTrue();
var err = result.AsT1;
err.Message.Should().Be("Price must be positive");