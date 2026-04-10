using OneOf;
using System;
using System.Threading.Tasks;

// 1. Records and types
public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

// 2. Repository Interface
public interface IProductRepository
{
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

// 3. Command Handler Interface
public interface ICommandHandler<TCommand, TResult>
{
    Task<TResult> HandleAsync(TCommand command);
}

// 4. Command Handler Implementation
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
    {
        // Validation 1: Name required
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new ValidationError("Name is required");
        }

        // Validation 2: Price positive
        if (command.Price <= 0)
        {
            return new ValidationError("Price must be positive");
        }

        // Validation 3: Duplicate check
        if (await _repository.ExistsAsync(command.Name))
        {
            return new DuplicateError(command.Name);
        }

        // Success path
        var newProduct = new Product(0, command.Name, command.Price);
        var addedProduct = await _repository.AddAsync(newProduct);
        return addedProduct;
    }
}

// --- Test/Usage Context (Not required for the final output, but useful for context) ---
/*
public class ProductServiceTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsProduct()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        mockRepo.ExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(false));
        mockRepo.AddAsync(Arg.Any<Product>()).Returns(new Product(1, "Test", 10.00m));

        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand("Test", 10.00m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().IsT0();
        var product = result.AsT0();
        product.Should().BeEquivalentTo(new Product(1, "Test", 10.00m));

        // Verify interaction
        await mockRepo.Received(1).AddAsync(Arg.Is<Product>(p => p.Name == "Test" && p.Price == 10.00m));
    }

    [Fact]
    public async Task HandleAsync_NullName_ReturnsValidationError()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand(null, 10.00m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().IsT1();
        var error = result.AsT1();
        error.Should().BeOfType<ValidationError>().And.HaveMessage("Name is required");

        // Verify no repository calls were made
        await mockRepo.DidNotReceive().ExistsAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsDuplicateError()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        mockRepo.ExistsAsync("Existing").Returns(Task.FromResult(true));
        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand("Existing", 10.00m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().IsT2();
        var error = result.AsT2();
        error.Should().BeOfType<DuplicateError>().And.HaveExistingName("Existing");
    }
}
*/