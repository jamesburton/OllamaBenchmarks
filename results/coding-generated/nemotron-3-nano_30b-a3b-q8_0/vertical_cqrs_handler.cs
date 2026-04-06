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
    public CreateProductCommandHandler(IProductRepository repository) => _repository = repository;

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

// Tests
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenNameIsNullOrWhiteSpace()
    {
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);
        var result = await handler.HandleAsync(new CreateProductCommand("   ", 10));
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationError_WhenPriceIsNegativeOrZero()
    {
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);
        var result = await handler.HandleAsync(new CreateProductCommand("Test", 0));
        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnDuplicateError_WhenProductExists()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Test").Returns(true);
        var handler = new CreateProductCommandHandler(repo);
        var result = await handler.HandleAsync(new CreateProductCommand("Test", 10));
        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Test");
    }

    [Fact]
    public async Task HandleAsync_ShouldAddProduct_WhenValid()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.AddAsync(Arg.Any<Product>()).Returns(System.Threading.Tasks.Task.FromResult(new Product(1, "Test", 12.34m)));
        repo.ExistsAsync("Test").Returns(false);
        var handler = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand("Test", 12.34m);
        var result = await handler.HandleAsync(command);
        result.IsT0.Should().BeTrue();
        var product = result.AsT0;
        product.Id.Should().BeGreaterThan(0);
        product.Name.Should().Be("Test");
        product.Price.Should().Be(12.34m);
    }
}