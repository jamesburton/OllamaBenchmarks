using System;
using System.Threading.Tasks;
using NSubstitute;
using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message)
{
    public static readonly ValidationError Empty = new(EmptyString);
}
public record DuplicateError(string ExistingName);

interface IProductRepository
{
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            return OneOfValidationError.Empty;

        if (command.Price <= 0)
            return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists)
            return new DuplicateError(command.Name);

        int id;
        Product product;
        bool success = await _repository.AddAsync(new Product(id, command.Name, command.Price));
        if (!success) throw new InvalidOperationException("Failed to add the product.");

        return OneOfProduct(product);
    }
}

// Example test using NSubstitute and AwesomeAssertions
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_CreatesValidProduct_ReturnsOneOfProduct()
    {
        var repository = Substitute.For<IProductRepository>();
        repository.AddAsync(Arg.Any<Product>());
        repository.ExistsAsync(Arg.IsAny<string>().Which(x => !string.IsNullOrWhiteSpace(x)));

        CreateProductCommandHandler handler = new(CreateProductCommandHandler(repository));
        OneOf<Product, ValidationError, DuplicateError> result =
            await Task.FromResult(handler.HandleAsync(new CreateProductCommand("Test Product", 9.99m)));

        result.Should().BeOneOfType<OneOf<Product>>();
    }

    [Fact]
    public async Task HandleAsync_ReturnsValidationError_ForEmptyName()
    {
        var repository = Substitute.For<IProductRepository>();

        CreateProductCommandHandler handler = new(CreateProductHandler(repository));
        OneOf<Product, ValidationError, DuplicateError> result =
            await Task.FromResult(handler.HandleAsync(new CreateProductCommand("", 9.99m)));

        result.Should().BeOneOfType<ValidationError>().Which(x => x is NotFound);
    }

    [Fact]
    public async Task HandleAsync_ReturnsDuplicateError_ForExistingName()
    {
        var repository = Substitute.For<IProductRepository>();

        // Simulate existing product
        int id;
        Product existingProduct = new(Product(1, "Test Product", 9.99m));
        await repository.AddAsync(existingProduct);

        CreateProductCommandHandler handler = new(CreateProductCommandHandler(repository));
        OneOf<Product, ValidationError, DuplicateError> result =
            await Task.FromResult(handler.HandleAsync(new CreateProductCommand("Test Product", 9.99m)));

        result.Should().BeOneOfType<DuplicateError>().Which(x => x.ExistingName == "Test Product");
    }
}