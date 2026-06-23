using OneOf;
using NSubstitute;
using AwesomeAssertions;

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

        var product = await _repository.AddAsync(new Product(0, command.Name, command.Price));
        return product;
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ReturnsProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("NewName").Returns(false);
        repository.AddAsync(Arg.Any<Product>()).Returns(new Product(1, "NewName", 9.99m));

        var handler = new CreateProductCommandHandler(repository);
        var result = await handler.HandleAsync(new CreateProductCommand("NewName", 9.99m));

        result.Should().BeEquivalentTo(new Product(1, "NewName", 9.99m), opt => opt.Excluding(x => x.Id));
    }

    [Fact]
    public async Task Handle_DuplicateName_ReturnsDuplicateError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync("ExistingName").Returns(true);

        var handler = new CreateProductCommandHandler(repository);
        var result = await handler.HandleAsync(new CreateProductCommand("ExistingName", 10m));

        result.Should().Be(new DuplicateError("ExistingName"));
    }

    [Fact]
    public async Task Handle_InvalidPrice_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);

        var handler = new CreateProductCommandHandler(repository);
        var result = await handler.HandleAsync(new CreateProductCommand("NewName", -1m));

        result.Should().Be(new ValidationError("Price must be positive"));
    }

    [Fact]
    public async Task Handle_NullName_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);

        var handler = new CreateProductCommandHandler(repository);
        var result = await handler.HandleAsync(new CreateProductCommand(null, 10m));

        result.Should().Be(new ValidationError("Name is required"));
    }

    [Fact]
    public async Task Handle_WhitespaceName_ReturnsValidationError()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);

        var handler = new CreateProductCommandHandler(repository);
        var result = await handler.HandleAsync(new CreateProductCommand("   ", 10m));

        result.Should().Be(new ValidationError("Name is required"));
    }
}