using OneOf;
using Microsoft.AspNetCore.Mvc;

public TResult Match<TResult, T0, T1, T2>(
    Func<T0, TResult> t0,
    Func<T1, TResult> t1,
    Func<T2, TResult> t2
)

result.Match<IActionResult>(
    user => Ok(user),
    _ => NotFound(),
    err => BadRequest(err.Message)
);

string display = result.Match(
    user => user.Name,
    notFound => "Not found"
);

public TResult Match<TResult, T0, T1, T2>(
    Func<T0, TResult> t0,
    Func<T1, TResult> t1,
    Func<T2, TResult> t2
)

return result.Match<IActionResult>(
    user => Ok(user),
    _ => NotFound(),
    err => BadRequest(err.Message)
);

public record NotFound;

public record ValidationError(string Message);

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>?> GetByIdAsync(int id);
}

public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return result.Match<IActionResult>(
            user => Ok(user),
            _ => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}

using OneOf;
using Microsoft.AspNetCore.Mvc;

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
    Task<OneOf<User, NotFound, ValidationError>?> GetByIdAsync(int id);
}

public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return result.Match<IActionResult>(
            user => Ok(user),
            _ => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}

public TResult Match<TResult, T0, T1, T2>(
    Func<T0, TResult> t0,
    Func<T1, TResult> t1,
    Func<T2, TResult> t2
)