using Microsoft.AspNetCore.Mvc;
using OneOf;

// Records for error types
public record NotFound(int Id);
public record ValidationError(string Message);

// User record
public record User(int Id, string Name, string Email);

// Service interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// Controller
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
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return result.Match<IActionResult>(
            user => Ok(user),
            _ => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}