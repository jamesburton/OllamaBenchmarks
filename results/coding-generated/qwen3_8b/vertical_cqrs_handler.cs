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

        var product = new Product(0, command.Name, command.Price);
        return await _repository.AddAsync(product);
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_InvalidName_ReturnsValidationError()
    {
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);

        var result = await handler.HandleAsync(new CreateProductCommand("", 100));

        result.Should().BeOfType<ValidationError>();
        result.AsT1().Message.Should().Be("Name is required");
    }

    [Fact]
    public async Task HandleAsync_NegativePrice_ReturnsValidationError()
    {
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);

        var result = await handler.Handle
            .HandleAsync(new CreateProductCommand("Laptop", -100));

        result.Should().BeOfType<ValidationError>();
        result.AsT1().Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsDuplicateError()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Laptop").Returns(true);
        var handler = new CreateProductCommandHandler(repo);

        var result = await handler.HandleAsync(new CreateProductCommand("Laptop", 100));

        result.Should().BeOfType<DuplicateError>();
        result.AsT2().ExistingName.Should().Be("Laptop");
    }

    [Fact]
    public async Task HandleAsync_ValidInput_ReturnsProduct()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Laptop").Returns(false);
        repo.AddAsync(Arg.Any<Product>()).Returns(Task.FromResult(new Product(1, "Laptop", 100)));
        var handler = new CreateProductCommandHandler(repo);

        var result = await handler.HandleAsync(new CreateProductCommand("Laptop", 100));

        result.Should().BeOfType<Product>();
        result.AsT0().Id.Should().Be(1);
        result.AsT0().Name.Should().Be("Laptop");
        result.AsT0().Price.Should().Be(100);
    }
}