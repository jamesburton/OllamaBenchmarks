using OneOf;
using NSubstitute;
using AwesomeAssertions;

// 1. Records and types
public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

// 2. IProductRepository interface
public interface IProductRepository
{
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

// 3. ICommandHandler interface
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
        _repository = repository;
    }

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command)
    {
        // Validation 1: Name check
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            return new ValidationError("Name is required");
        }

        // Validation 2: Price check
        if (command.Price <= 0)
        {
            return new ValidationError("Price must be positive");
        }

        // Check for duplicates
        bool exists = await _repository.ExistsAsync(command.Name);
        if (exists)
        {
            return new DuplicateError(command.Name);
        }

        // Add product
        var newProduct = new Product(0, command.Name, command.Price);
        await _repository.AddAsync(newProduct);

        // Return the created product
        return newProduct;
    }
}

// Example Test Setup (Demonstration purposes, not required in final output, but shows usage)
/*
public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsProduct()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        mockRepo.ExistsAsync(Arg.Any<string>()).Returns(Task.FromResult(false));
        mockRepo.AddAsync(Arg.Any<Product>()).Returns(Task.CompletedTask);

        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand("Test", 10.99m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<Product>();
        result.IsT0.Should().NotBeNull();
        result.IsT1.Should().BeNull();
        result.IsT2.Should().BeNull();
        result.AsT0.Should().NotBeNull();
        result.AsT0.Should().HaveProperty(p => p.Name == "Test");
    }

    [Fact]
    public async Task HandleAsync_DuplicateName_ReturnsDuplicateError()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        mockRepo.ExistsAsync("DuplicateItem").Returns(Task.FromResult(true));

        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand("DuplicateItem", 10.99m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<DuplicateError>();
        result.IsT0.Should().BeNull();
        result.IsT1.Should().NotBeNull();
        result.AsT1.Should().NotBeNull();
        result.AsT1.Should().HaveProperty(d => d.ExistingName == "DuplicateItem");
    }

    [Fact]
    public async Task HandleAsync_NullName_ReturnsValidationError()
    {
        // Arrange
        var mockRepo = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(mockRepo);
        var command = new CreateProductCommand(null, 10.99m);

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.Should().BeOfType<ValidationError>();
        result.IsT0.Should().BeNull();
        result.IsT1.Should().NotBeNull();
        result.AsT1.Should().NotBeNull();
        result.AsT1.Should().HaveProperty(e => e.Message == "Name is required");
    }
}
*/