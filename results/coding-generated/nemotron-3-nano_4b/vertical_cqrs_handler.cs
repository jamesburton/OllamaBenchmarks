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
    private readonly IProductRepository repository;

    public CreateProductCommandHandler(IProductRepository repository) => this.repository = repository;

    public Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return Task.FromResult(new ValidationError("Name is required"));
        }

        if (command.Price <= 0)
        {
            return Task.FromResult(new ValidationError("Price must be positive"));
        }

        var existsAsync = repository.ExistsAsync(command.Name);
        return Task.FromResult( existsAsync
            ? new DuplicateError(command.Name)
            : await Task.FromResult(repository.AddAsync(new Product(0, command.Name, command.Price)))
        );
    }
}