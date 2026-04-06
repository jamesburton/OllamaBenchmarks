using System;
using System.Threading.Tasks;
using OneOf;
using NSubstitute;
using AwesomeAssertions;

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
            return new ValidationError("Name is required");

        if (command.Price <= 0)
            return new ValidationError("Price must be positive");

        if (await _repository.ExistsAsync(command.Name))
            return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        await _repository.AddAsync(product);
        return product;
    }
}

public class CreateProductCommandHandlerTests
{
    [Fact]
    public void Handle_ValidCommand_ReturnsProduct()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(false);
        repository.AddAsync(Arg.Any<Product>()).Returns(new Product(1, "Test", 10m));
        var handler = new CreateProductCommandHandler(repository);

        var command = new CreateProductCommand("Test", 10m);

        // Act
        var result = handler.HandleAsync(command).Result;

        // Assert
        result.Should().BeOfType<Product>();
        var product = result.AsT0;
        product.Id.Should().Be(1);
        product.Name.Should().Be("Test");
        product.Price.Should().Be(10m);
    }

    [Fact]
    public void Handle_EmptyName_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        var command = new CreateProductCommand("", 10m);

        // Act
        var result = handler.HandleAsync(command).Result;

        // Assert
        result.Should().BeOfType<ValidationError>();
        var error = result.AsT1;
        error.Message.Should().Be("Name is required");
    }

    [Fact]
    public void Handle_NegativePrice_ReturnsValidationError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        var handler = new CreateProductCommandHandler(repository);

        var command = new CreateProductCommand("Test", -1m);

        // Act
        var result = handler.HandleAsync(command).Result;

        // Assert
        result.Should().BeOfType<ValidationError>();
        var error = result.AsT1;
        error.Message.Should().Be("Price must be positive");
    }

    [Fact]
    public void Handle_DuplicateName_ReturnsDuplicateError()
    {
        // Arrange
        var repository = Substitute.For<IProductRepository>();
        repository.ExistsAsync(Arg.Any<string>()).Returns(true);
        var handler = new CreateProductCommandHandler(repository);

        var command = new CreateProductCommand("Test", 10m);

        // Act
        var result = handler.HandleAsync(command).Result;

        // Assert
        result.Should().BeOfType<DuplicateError>();
        var error = result.AsT2;
        error.ExistingName.Should().Be("Test");
    }
}