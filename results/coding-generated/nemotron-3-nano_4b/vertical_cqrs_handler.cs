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

public partial class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>>
{
    private readonly IProductRepository _repo;

    public CreateProductCommandHandler(IProductRepository repo) => _repo = repo;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return new ValidationError("Name is required").Return<ValidationError>();

        if (command.Price <= 0)
            return new DuplicateError($"Duplicate for name {command.Name}").Return<DuplicateError>();

        if (await _repo.ExistsAsync(command.Name))
            return new DuplicateError($"Duplicate for name {command.Name}").Return<DuplicateError>();

        var product = new Product(0, command.Name, command.Price);
        var result = await _repo.AddAsync(product).Result;
        return result.Return();
    }
}