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

public class CreateProductCommandHandler :
    ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>>
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

// ---------- Tests ----------
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Should_return_validation_error_when_name_is_null_or_whitespace()
    {
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);

        var command = new CreateProductCommand("   ", 10m);
        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task Should_return_validation_error_when_price_is_non_positive()
    {
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);

        var command = new CreateProductCommand("Valid Name", 0m);
        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task Should_return_duplicate_error_when_product_exists()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Existing Product").Returns(true);

        var handler = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand("Existing Product", 15m);
        var result = await handler.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing Product");
    }

    [Fact]
    public async Task Should_create_product_successfully()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync(Arg.Any<string>()).Returns(false);
        repo.AddAsync(Arg.Any<Product>())
            .Returns(callInfo =>
            {
                var p = callInfo.Arg<Product>();
                return Task.FromResult(p with { Id = 1 });
            });

        var handler = new CreateProductCommandHandler(repo);
        var command = new CreateProductCommand("New Product", 20m);
        var result = await handler.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        var product = result.AsT0;
        product.Id.Should().Be(1);
        product.Name.Should().Be("New Product");
        product.Price.Should().Be(20m);
    }
}