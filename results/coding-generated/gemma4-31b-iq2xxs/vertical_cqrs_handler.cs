*   Goal: Create a CQRS command handler vertical slice.
    *   Records: `CreateProductCommand`, `Product`, `ValidationError`, `DuplicateError`.
    *   Interface: `IProductRepository` (methods: `ExistsAsync`, `AddAsync`).
    *   Interface: `ICommandHandler<TCommand, TResult>` (method: `HandleAsync`).
    *   Implementation: `CreateProductCommandHandler`.
    *   Logic:
        *   Null/Whitespace Name -> `ValidationError`.
        *   Price <= 0 -> `ValidationError`.
        *   ExistsAsync -> `DuplicaateError`.
        *   Success -> `AddAsync` and return `Product`.
    *   Constraints:
        *   `using OneOf;` at the top.
        *   No other `using` statements (except global usings).
        *   No namespaces.
        *   Use `NSubsitute` for mocks.
        *   Use `AwesomeAssertions` for assertions.
        *   Use `OneOf` properties (`IsT0`, `AsT0`, etc.).

    *   `OneOf<Product, ValidationError, Duplica1Error>`’s structure:
        *   T0: `Product`
        *   T1: `ValidationError`
        *   T2: `Duplica1Error` (Wait, the prompt says `DuplicaateError` - typo in prompt "DuplicaateError" vs "DuplicaateError", I'll use the spelling provided in the prompt: `Duplica