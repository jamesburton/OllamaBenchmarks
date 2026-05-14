using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OneOf;

record User(int Id, string Name, string Email);
record NotFound;
record ValidationError(string Message);

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

using OneOf;

public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IOptions<SmtpOptions> _smtpOptions;

    public UsersController(IUserService userService, IOptions<SmtpOptions> smtpOptions)
    {
        _userService = userService;
        _smtpOptions = smtpOptions;
    }

    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return result.Match(
            user => Ok(user),
            _ => NotFound(),
            ValidationError(msg) => BadRequest(new { error = msg })
        );
    }
}

// Example usage assumed in service
public partial class UserService : IUserService
{
    public async Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        // Example logic returning based on conditions
        if (id < 0)
            return OneOf<User, NotFound, ValidationError>.FromT1(new ValidationError($"Invalid ID: {id}"));
        else if (id == 999)
            return OneOf<User, NotFound, ValidationError>.FromT1(new NotFound());
        else
            return OneOf<User, NotFound, ValidationError>.FromT0(new User(id, "Alice", "a@b.com"));
    }
}