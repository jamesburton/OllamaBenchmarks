using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace OneOfAspNetCoreDemo;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public record NotFound;

public record ValidationError(string Message);

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

[ApiController]
[Route("api/[controller]")]
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

        return result.Match(
            user => Ok(user),
            _ => NotFound(),
            error => BadRequest(error.Message)
        );
    }
}