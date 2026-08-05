using Microsoft.AspNetCore.Mvc;
using OneOf;

// User record with public properties for object initializer syntax
public class User 
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}

// NotFound record with parameterless constructor
public record NotFound;

// ValidationError record accepting a single Message parameter
public record ValidationError(string Message);

// IUserService interface defining the service contract
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
            notFound => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}