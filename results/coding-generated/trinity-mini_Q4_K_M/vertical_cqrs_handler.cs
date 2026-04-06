using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}

using OneOf;

public record CreateProductCommand(string Name, decimal Price);
public record Product(int Id, string Name, decimal Price);
public record ValidationError(string Message);
public record DuplicateError(string ExistingName);

interface IProductRepository {
    Task<bool> ExistsAsync(string name);
    Task<Product> AddAsync(Product product);
}

interface ICommandHandler<TCommand, TResult> {
    Task<TResult> HandleAsync(TCommand command);
}

class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, OneOf<Product, ValidationError, DuplicateError>> {
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository) => 
        _repository = repository;

    public async Task<OneOf<Product, ValidationError, DuplicateError>> HandleAsync(CreateProductCommand command) {
        if (string.IsNullOrWhiteSpace(command.Name)) return new ValidationError("Name is required");
        if (command.Price <= 0m) return new ValidationError("Price must be positive");

        var exists = await _repository.ExistsAsync(command.Name);
        if (exists) return new DuplicateError(command.Name);

        var product = new Product(0, command.Name, command.Price);
        var result = await _repository.AddAsync(product);
        return result;
    }
}