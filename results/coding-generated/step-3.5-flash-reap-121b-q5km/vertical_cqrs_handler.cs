using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

public interface IProductRepository
{
    System.Threading.Tasks.Task<bool> ExistsAsync(string name);
    System.Threading.Tasks.Task<Product> AddAsync(Product product);
}

public interface ICommandHandler<TCommand, TResult>
{
    System.Threading.Tasks.Task<TResult> HandleAsync(TCommand command);
}

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async System.Threading.Tasks.Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
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
    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_WhenNameIsNull_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand(null, 10);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_WhenPriceIsZero_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Product", 0);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_WhenProductWithSameNameExists_ReturnsDuplicateError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Is("Product")).Returns(true);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Product", 10);

        var result = await handler.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Product");
    }

    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_WhenValidCommand_ReturnsProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        var product = new Product(1, "Product", 10);
        repository.AddAsync(Arg.Any<Product>()).Returns(product);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Product", 10);

        var result = await handler.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().BeEquivalentTo(product);
    }
}