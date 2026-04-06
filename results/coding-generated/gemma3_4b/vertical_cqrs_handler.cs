using OneOf;
using AwesomeAssertions;
using NSubstitute;

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

        if (await _repository.ExistsAsync(command.Name) is true)
        {
            return new DuplicateError(command.Name);
        }

        var product = new Product(0, command.Name, command.Price);
        var addedProduct = await _repository.AddAsync(product);

        return addedProduct;
    }
}

public interface IProductRepository
{
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

public record CreateProductCommand(string Name, decimal Price)
{
}

public record Product(int Id, string Name, decimal Price)
{
}

public record ValidationError(string Message)
{
}

public record DuplicateError(string ExistingName)
{
}

public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command);
}