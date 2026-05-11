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
        var added = await _repository.AddAsync(product);
        return added;
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_NameIsNullOrWhitespace_ReturnsValidationError()
    {
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);

        var result = await handler.HandleAsync(new CreateProductCommand(null, 10));

        result.IsT1.Should().BeTrue();
        result.AsT1.Should().Be(new ValidationError("Name is required"));

        await repo.DidNotReceive().ExistsAsync(Arg.Any<string>());
        await repo.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_PriceIsZeroOrNegative_ReturnsValidationError()
    {
        var repo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repo);

        var result = await handler.HandleAsync(new CreateProductCommand("Widget", 0));

        result.IsT1.Should().BeTrue();
        result.AsT1.Should().Be(new ValidationError("Price must be positive"));

        await repo.DidNotReceive().ExistsAsync(Arg.Any<string>());
        await repo.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_ProductAlreadyExists_ReturnsDuplicateError()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Widget").Returns(Task.FromResult(true));
        var handler = new CreateProductCommandHandler(repo);

        var result = await handler.HandleAsync(new CreateProductCommand("Widget", 10));

        result.IsT2.Should().BeTrue();
        result.AsT2.Should().Be(new DuplicateError("Widget"));

        await repo.Received(1).ExistsAsync("Widget");
        await repo.DidNotReceive().AddAsync(Arg.Any<Product>());
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_AddsProductAndReturnsIt()
    {
        var repo = Substitute.For<IProductRepository>();
        repo.ExistsAsync("Widget").Returns(Task.FromResult(false));
        var addedProduct = new Product(1, "Widget", 10);
        repo.AddAsync(Arg.Is<Product>(p => p.Name == "Widget" && p.Price == 10 && p.Id == 0))
            .Returns(Task.FromResult(addedProduct));

        var handler = new CreateProductCommandHandler(repo);

        var result = await handler.HandleAsync(new CreateProductCommand("Widget", 10));

        result.IsT0.Should().BeTrue();
        result.AsT0.Should().Be(addedProduct);

        await repo.Received(1).ExistsAsync("Widget");
        await repo.Received(1).AddAsync(Arg.Is<Product>(p => p.Name == "Widget" && p.Price == 10 && p.Id == 0));
    }
}