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
            return OneOf<Product, ValidationError, DuplicateError>.FromT1(new ValidationError("Name is required"));

        if (command.Price <= 0)
            return OneOf<Product, ValidationError, DuplicateError>.FromT1(new ValidationError("Price must be positive"));

        if (await _repository.ExistsAsync(command.Name))
            return OneOf<Product, ValidationError, DuplicateError>.FromT2(new DuplicateError(command.Name));

        var product = await _repository.AddAsync(new Product(0, command.Name, command.Price));
        return OneOf<Product, ValidationError, DuplicateError>.FromT0(product);
    }
}