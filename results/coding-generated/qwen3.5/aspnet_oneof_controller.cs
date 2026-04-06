using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using OneOf;

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public record NotFound;

    public record ValidationError(string Message);

    public interface IUserService
    {
        Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
    }

    public class UsersController : ControllerBase
    {
        private readonly IUserService _service;

        public UsersController(IUserService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return result.Match<IActionResult>(
                user => Ok(user),
                notFound => NotFound(),
                err => BadRequest(err.Message)
            );
        }
    }

return result.Match<IActionResult>(
        user    => Ok(user),
        _       => NotFound(),
        err     => BadRequest(err.Message)
    );

return result.Match<IActionResult>(
        user    => Ok(user),
        _       => NotFound(),
        err     => BadRequest(err.Message)
    );

using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using OneOf;

    public class User { ... }
    public record NotFound;
    public record ValidationError(string Message);
    public interface IUserService { ... }
    public class UsersController : ControllerBase { ... }

return result.Match<IActionResult>(
        user    => Ok(user),
        _       => NotFound(),
        err     => BadRequest(err.Message)
    );

.
    I will do that.

    Wait, checking the constraint "Do NOT place any types inside a namespace".
    This means no `namespace` keyword.
    So the file starts with `using`.

    Okay.

    Wait, one detail: `OneOf` library might require `OneOf` namespace or `using OneOf;`. The reference says `using OneOf;`. I will use that.

    Okay, final plan:
    1. Imports.
    2. `User` class.
    3. `NotFound` record.
    4. `ValidationError` record.
    5. `IUserService` interface.
    6. `UsersController` class.
    7. `Get` method.

    Let's write it.

    Wait, I need to check if `OneOf` types need to be imported specifically or if `OneOf<User>` works. The reference shows `OneOf<User, NotFound>`. This implies `OneOf` is a namespace or the types are in the `OneOf` namespace. The reference says `using OneOf;`. So `OneOf<T>` works.

    Okay.

    Wait, one detail: `BadRequest(err.Message)`. `BadRequest` is a static method in `Microsoft.AspNetCore.Mvc.Results`.
    `NotFound()` is a static method.
    `Ok(user)` is a static method.
    `Match<IActionResult>` is an extension method.

    Okay.

    Wait, I need to check if `OneOf` is a namespace or a package. The reference says `Package: OneOf`. `using OneOf;`.
    So `OneOf<User>` is valid.

    Okay, writing the code.

    Wait, one last check: "Return only valid C# code in a single file."
    I will output the code block.

    Wait, I should check if `IUserService` needs to be public. Yes, interfaces are usually public.
    `UsersController` needs to be public.
    `User`, `NotFound`, `ValidationError` need to be public.

    Okay.

    Wait, one detail: `using System;` is needed for `int`, `string`.
    `using System.Threading.Tasks;` for `Task`.
    `using Microsoft.AspNetCore.Mvc;` for `ControllerBase`, `IActionResult`, `Ok`, `NotFound`, `BadRequest`.
    `using OneOf;` for `OneOf`.

    Okay.

    Wait, I need to make sure I don't use `namespace`.
    So:

Okay.

    Wait, one detail: `Get` method must be async. `public async Task<IActionResult> Get(int id)`.
    Okay.

    Wait, one detail: `IUserService` interface.
    `public interface IUserService`
    `Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);`

    Okay.

    Wait, one detail: `User` class.
    `public class User`
    `public int Id { get; set; }`
    `public string Name { get; set; }`
    `public string Email { get; set; }`

    Okay.

    Wait, one detail: `NotFound` record.
    `public record NotFound;`

    Okay.

    Wait, one detail: `ValidationError` record.
    `public record ValidationError(string Message);`

    Okay.

    Wait, one detail: `UsersController`.
    `public class UsersController : ControllerBase`
    `private readonly IUserService _service;`
    `public UsersController(IUserService service)`
    `public async Task<IActionResult> Get(int id)`

    Okay.

    Wait, one detail: `Match<IActionResult>` usage.
    `return result.Match<IActionResult>(user => Ok(user), notFound => NotFound(), err => BadRequest(err.Message));`
    Wait, the reference uses `_` for the second case.
    `user => Ok(user), _ => NotFound(), err => BadRequest(err.Message)`
    I should follow the reference pattern for consistency, but `_` is fine.
    The reference shows: