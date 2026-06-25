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
        {
            return OneOf<Product, ValidationError, DuplicateError>.FromT1(new ValidationError("Name is required"));
        }

        if (command.Price <= 0)
        {
            return OneOf<Product, ValidationError, DuplicateError>.FromT1(new ValidationError("Price must be positive"));
        }

        if (await _repository.ExistsAsync(command.Name))
        {
            return OneOf<Product, ValidationError, DuplicateError>.FromT2(new DuplicateError(command.Name));
        }

        var product = await _repository.AddAsync(new Product(0, command.Name, command.Price));
        return OneOf<Product, ValidationError, DuplicateError>.FromT0(product);
    }
}

public class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _repository = Substitute.For<IProductRepository>();
    private readonly CreateProductCommandHandler _handler = new_CreateProductCommandHandler(_repository);

    public async Task HandleAsync_ValidCommand_ReturnsProduct()
    {
        var command = new CreateProductCommand("Gadget", 10.0m);
        _repository.ExistsAsync(command.Name).Returns(false);
        _repository.AddAsync(Arg.Any<Product>()).Returns(new Product(1, "Gadget", 10.0m));

        var result = await _handler.HandleAsync(command);

        result.IsT0.Should().BeTrue();
        result.AsT0.Name.Should().Be("Gadget");
    }

    public async Task HandleAsync_EmptyName_ReturnsValidationError()
    {
        var command = new CreateProductCommand("", 10.0m);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Name is required");
    }

    public async Task HandleAsync_NegativePrice_ReturnsValidationError()
    {
        var command = new CreateProductCommand("Gadget", -5m);
        var result = await _handler.HandleAsync(command);

        result.IsT1.Should().BeTrue();
        result.AsT1.Message.Should().Be("Price must be positive");
    }

    public async Task HandleAsync_DuplicateName_ReturnsDuplicateError()
    {
        var command = new CreateProductCommand("Existing", 10.0m);
        _repository.ExistsAsync(command.Name).Returns(true);

        var result = await _handler.HandleAsync(command);

        result.IsT2.Should().BeTrue();
        result.AsT2.ExistingName.Should().Be("Existing");
    }
}