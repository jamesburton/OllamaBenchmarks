using OneOf;
using Microsoft.AspNetCore.Mvc;

public record NotFound;
public record ValidationError(string Message);

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return result.Match<IActionResult>(
            user => Ok(user),
            _ => NotFound(),
            error => BadRequest(error.Message)
        );
    }
}