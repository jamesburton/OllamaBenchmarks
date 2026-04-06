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

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError> 
{
    private readonly IProductRepository _repo;

    public CreateProductCommandHandler(IProductRepository repo) 
    {
        _repo = repo;
    }

    public async Task<Task<bool>> HandleAsync(CreateProductCommand command)
    {
        if (command.Name == null || command.Name.Length == 0) 
            return Task.FromResult(CreateProductCommandHandler.Result);
        if (command.Price <= 0) 
            return Task.FromResult(CreateProductCommandHandler.Result);
        if (await _repo.ExistsAsync(command.Name))
            return Task.FromResult(CreateProductCommandHandler.Result);
        return await _repo.AddAsync(new Product(0, command.Name, command.Price));
    }
}