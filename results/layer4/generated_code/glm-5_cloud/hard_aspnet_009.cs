using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace VerticalSlice
{
    // --- Shared Infrastructure ---

    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public record ValidationResult(bool IsValid, IReadOnlyList<string> Errors);

    /// <summary>
    /// Contract for validating objects of type T.
    /// </summary>
    public interface IValidator<T>
    {
        ValidationResult Validate(T instance);
    }

    /// <summary>
    /// Contract for handling commands/queries.
    /// </summary>
    public interface ICommandHandler<TCommand, TResult>
    {
        Task<TResult> HandleAsync(TCommand command, CancellationToken ct);
    }

    // --- Domain Models & DTOs ---

    public record ProductDto(int Id, string Name, string Category, decimal Price);
    public record ValidationError(IReadOnlyList<string> Errors);
    public record NotFoundError(int Id);

    // --- Commands & Queries ---

    public record CreateProductCommand(string Name, string Category, decimal Price);
    public record GetProductQuery(int Id);

    // --- Validators ---

    public class CreateProductCommandValidator : IValidator<CreateProductCommand>
    {
        public ValidationResult Validate(CreateProductCommand instance)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(instance.Name))
                errors.Add("Name must not be empty.");

            if (string.IsNullOrWhiteSpace(instance.Category))
                errors.Add("Category must not be empty.");

            if (instance.Price <= 0)
                errors.Add("Price must be greater than 0.");

            return new ValidationResult(errors.Count == 0, errors);
        }
    }

    public class GetProductQueryValidator : IValidator<GetProductQuery>
    {
        public ValidationResult Validate(GetProductQuery instance)
        {
            var errors = new List<string>();

            if (instance.Id <= 0)
                errors.Add("Id must be greater than 0.");

            return new ValidationResult(errors.Count == 0, errors);
        }
    }

    // --- Data Store ---

    /// <summary>
    /// Singleton in-memory store for products.
    /// </summary>
    public class InMemoryProductStore
    {
        private int _idCounter = 0;
        private readonly List<ProductDto> _products = new();

        public ProductDto Add(string name, string category, decimal price)
        {
            var newId = Interlocked.Increment(ref _idCounter);
            var product = new ProductDto(newId, name, category, price);
            _products.Add(product);
            return product;
        }

        public ProductDto? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);
    }

    // --- Handlers ---

    public class CreateProductHandler : ICommandHandler<CreateProductCommand, OneOf<ProductDto, ValidationError>>
    {
        private readonly IValidator<CreateProductCommand> _validator;
        private readonly InMemoryProductStore _store;

        public CreateProductHandler(IValidator<CreateProductCommand> validator, InMemoryProductStore store)
        {
            _validator = validator;
            _store = store;
        }

        public Task<OneOf<ProductDto, ValidationError>> HandleAsync(CreateProductCommand command, CancellationToken ct)
        {
            var validationResult = _validator.Validate(command);

            if (!validationResult.IsValid)
            {
                return Task.FromResult<OneOf<ProductDto, ValidationError>>(new ValidationError(validationResult.Errors));
            }

            var product = _store.Add(command.Name, command.Category, command.Price);
            return Task.FromResult<OneOf<ProductDto, ValidationError>>(product);
        }
    }

    public class GetProductHandler : ICommandHandler<GetProductQuery, OneOf<ProductDto, NotFoundError>>
    {
        private readonly IValidator<GetProductQuery> _validator;
        private readonly InMemoryProductStore _store;

        public GetProductHandler(IValidator<GetProductQuery> validator, InMemoryProductStore store)
        {
            _validator = validator;
            _store = store;
        }

        public Task<OneOf<ProductDto, NotFoundError>> HandleAsync(GetProductQuery query, CancellationToken ct)
        {
            var validationResult = _validator.Validate(query);

            if (!validationResult.IsValid)
            {
                // For this specific handler signature, we treat validation failure as a NotFound logic error 
                // (or one could argue the signature should allow ValidationErrors, but adhering strictly to the prompt signature).
                // However, usually validation errors precede lookup. 
                // If we must return NotFoundError per prompt signature, we return a NotFound with the invalid ID.
                return Task.FromResult<OneOf<ProductDto, NotFoundError>>(new NotFoundError(query.Id));
            }

            var product = _store.GetById(query.Id);

            if (product == null)
            {
                return Task.FromResult<OneOf<ProductDto, NotFoundError>>(new NotFoundError(query.Id));
            }

            return Task.FromResult<OneOf<ProductDto, NotFoundError>>(product);
        }
    }

    // --- OneOf Implementation (Minimalist) ---

    /// <summary>
    /// Simple OneOf discriminated union to handle either T0 or T1 result.
    /// </summary>
    public readonly struct OneOf<T0, T1>
    {
        private readonly object? _value;
        private readonly int _index;

        private OneOf(object? value, int index)
        {
            _value = value;
            _index = index;
        }

        public static implicit operator OneOf<T0, T1>(T0 value) => new(value, 0);
        public static implicit operator OneOf<T0, T1>(T1 value) => new(value, 1);

        public TResult Match<TResult>(Func<T0, TResult> f0, Func<T1, TResult> f1)
        {
            return _index switch
            {
                0 => f0((T0)_value!),
                1 => f1((T1)_value!),
                _ => throw new InvalidOperationException()
            };
        }
    }
}