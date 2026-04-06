using OneOf;
using NSubstitute;
using System;
using System.Threading.Tasks;
using AwesomeAssertions;

namespace CQRSExample
{
    // 1. Records and types
    public record CreateProductCommand(string Name, decimal Price)
    {
        public static CreateProductCommand From(string name, decimal price)
        {
            return new CreateProductCommand(name, price);
        }
    }

    public record Product(int Id, string Name, decimal Price)
    {
        public static Product From(int id, string name, decimal price)
        {
            return new Product(id, name, price);
        }
    }

    public record ValidationError(string Message)
    {
        public static ValidationError From(string message)
        {
            return new ValidationError(message);
        }
    }

    public record DuplicateError(string ExistingName)
    {
        public static DuplicateError From(string name)
        {
            return new DuplicateError(name);
        }
    }

    // 2. IProductRepository interface
    public interface IProductRepository
    {
        Task<bool> ExistsAsync(string name);
        Task<Product> AddAsync(Product product);
    }

    // 3. Generic ICommandHandler interface
    public interface ICommandHandler<TCommand, TResult>
    {
        Task<TResult> HandleAsync(TCommand command);
    }

    // 4. CreateProductCommandHandler implementation
    public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>>
    {
        private readonly IProductRepository _repository;

        public CreateProductCommandHandler(IProductRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
        {
            // Validate command
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return ValidationError.From("Name is required");
            }

            if (command.Price <= 0)
            {
                return ValidationError.From("Price must be positive");
            }

            // Check for duplicates
            bool exists = await _repository.ExistsAsync(command.Name);
            if (exists)
            {
                return DuplicateError.From(command.Name);
            }

            // Create and save product
            var product = Product.From(0, command.Name, command.Price);
            var result = await _repository.AddAsync(product);

            return result;
        }
    }
}