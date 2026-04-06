using OneOf;
import AwesomeAssertions;
import NSubstitute;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

public interface IProductRepository
{
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

public interface ICommandHandler<TCommand, TResult> where TCommand : class
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

// Example usage / test (not part of the slice, but for illustration)
/*
var repo = Substitute.For<IProductRepository>();
var handler = new CreateProductCommandHandler(repo);

await handler.HandleAsync(new CreateProductCommand("", 10m))
    .Should().Be(OfType<ValidationError>())
    .And.Contain(e => e.Message == "Name is required");

await handler.HandleAsync(new CreateProductCommand("Test", -1m))
    .Should().Be(OfType<ValidationError>())
    .And.Contain(e => e.Message == "Price must be positive");

await handler.HandleAsync(new CreateProductCommand("Existing", 10m))
    .Should().Be(OfType<DuplicateError>())
    .And.Contain(e => e.ExistingName == "Existing");
*/
*/