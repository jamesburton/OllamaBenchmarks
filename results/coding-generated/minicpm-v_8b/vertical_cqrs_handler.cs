using AwesomeAssertions;
using FluentAssertions;
using NSubstitute;
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

        var existing = await _repository.ExistsAsync(command.Name);
        if (existing)
        {
            return new DuplicateError(command.Name);
        }

        var product = new Product(0, command.Name, command.Price);
        await _repository.AddAsync(product);

        return OneOf<Product>.FromT1(product);
    }
}

public class TestCreateProductCommandHandler
{
    [Fact]
    public async Task ShouldHandleCommand()
    {
        // Arrange

        var sut = Substitute.For<ICreateProductCommandHandler>();

        var command = new CreateProductCommand("Test", 5m);

        sut.HandleAsync(command).Returns(new Product(1, "Test", 5));

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<Product>();
    }

    [Fact]
    public async Task ShouldHandleDuplicateName()
    {
        // Arrange

        var sut = Substitute.For<ICreateProductCommandHandler>();

        var command = new CreateProductCommand("Test", 5m);
        await sut.HandleAsync(command).Returns(new Product(1, "Test", 5));

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<DuplicateError>();
    }

    [Fact]
    public async Task ShouldHandleNegativePrice()
    {
        // Arrange

        var sut = Substitute.For<ICreateProductCommandHandler>();

        var command = new CreateProductCommand("Test", -5m);
        await sut.HandleAsync(command).Returns(new Product(1, "Test", 0));

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Should().BeOfType<ValidationError>();
    }
}