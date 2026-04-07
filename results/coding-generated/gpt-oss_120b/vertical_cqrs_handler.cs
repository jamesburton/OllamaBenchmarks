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

    public CreateProductCommandHandler(IProductRepository repository) => _repository = repository;

    public async System.Threading.Tasks.Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
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

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_ShouldReturnValidationError_WhenNameIsNullOrWhitespace()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("", 10m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_ShouldReturnValidationError_WhenPriceIsNonPositive()
    {
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Valid Name", 0m);

        var result = await handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_ShouldReturnDuplicateError_WhenProductExists()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("Existing Product").Returns(true);
        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("Existing Product", 15m);

        var result = await handler.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing Product");
    }

    [Fact]
    public async System.Threading.Tasks.Task HandleAsync_ShouldCreateProduct_WhenValid()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        var addedProduct = new Product(1, "New Product", 20m);
        repository.AddAsync(Arg.Any<Product>()).Returns(addedProduct);

        var handler = new CreateProductCommandHandler(repository);
        var command = new CreateProductCommand("New Product", 20m);

        var result = await handler.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        var product = result.AsT0;
        product.Id.Should().Be(1);
        product.Name.Should().Be("New Product");
        product.Price.Should().Be(20m);

        repository.Received(1).ExistsAsync("New Product");
        repository.Received(1).AddAsync(Arg.Is<Product>(p => p.Name == "New Product" && p.Price == 20m));
    }
}