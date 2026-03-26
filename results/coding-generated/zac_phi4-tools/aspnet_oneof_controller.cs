using Microsoft.AspNetCore.Mvc;
using OneOf;

// Define User class with properties Id, Name, and Email
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Define NotFound record type
public record NotFound;

// Define ValidationError record type with a message property
public record ValidationError(string Message);

// Define IUserService interface with GetByIdAsync method
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// UsersController implementing the required functionality
[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET /api/users/{id}
    [HttpGet("{id}")]
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