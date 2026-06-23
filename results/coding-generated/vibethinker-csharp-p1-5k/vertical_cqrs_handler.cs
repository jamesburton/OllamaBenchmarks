using OneOf;

public record CreateProductCommand(string Name, decimal Price)
{
            Id = Guid.NewGuid().ToString();
        }

public record Product(int Id, string Name, decimal Price)
{
            Id = Id;
        }

public record ValidationError(string Message)
{
            Message = Message;
        }

public record DuplicateError(string ExistingName)
{
            ExistingName = ExistingName;
        }

public interface IProductRepository
{
            Task<bool> ExistsAsync(string name);
            Task<Product> AddAsync(Product product);
        }

public interface ICommandHandler<TCommand, outcome> where TCommand : Command
{
            Task<outcome> HandleAsync(TCommand command);
        }

public class CreateProductCommandHandler
{
            private readonly IProductRepository repository;

            public CreateProductCommandHandler(IProductRepository repository)
            {
                repository = repository;
            }

            public async Task<Outcome> HandleAsync(CreateProductCommand command)
            {
                if (string.IsNullOrWhiteSpace(command.Name))
                    return new ValidationError("Name is required");

                if (command.Price <= 0)
                    return new ValidationError("Price must be positive");

                var existing = await repository.ExistsAsync(command.Name);

                if (existing)
                    return new DuplicateError(command.Name);

                var product = new Product(0, command.Name, command.Price);
                await repository.AddAsync(product);

                return OneOf<Product, ValidationError, DuplicateError>(product, new ValidationError("Product already exists"), new DuplicateError(product.Name));
            }
        }