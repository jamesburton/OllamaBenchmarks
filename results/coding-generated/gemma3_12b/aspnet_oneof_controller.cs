using Microsoft.AspNetCore.Mvc;
using OneOf;
using System.Threading.Tasks;

public record User(int Id, string Name, string Email);

public record NotFound;

public record ValidationError(string Message);

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
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
        OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);
        return result.Match<IActionResult>(
            user => Ok(user),
            _ => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}