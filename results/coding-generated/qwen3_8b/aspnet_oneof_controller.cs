using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// User class with public properties for object initializer syntax
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// NotFound record with parameterless constructor
public record NotFound;

// ValidationError record with Message parameter
public record ValidationError(string Message);

// IUserService interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// UsersController
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // Controller action named Get
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