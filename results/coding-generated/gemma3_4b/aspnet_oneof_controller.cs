using Microsoft.AspNetCore.Mvc;
using OneOf;

// Define the User record
public record User(int Id, string Name, string Email);

// Define the NotFound record
public record NotFound;

// Define the ValidationError record
public record ValidationError(string Message);

// Define the IUserService interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// Define the UserService class
public class UserService : IUserService
{
    // Mock implementation for demonstration purposes
    public async Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        if (id == 1)
        {
            return new User(1, "Alice", "a@b.com");
        }
        else if (id == 2)
        {
            return new NotFound();
        }
        else
        {
            return new ValidationError("User not found");
        }
    }
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

        if (result is User user)
        {
            return Ok(user);
        }
        else if (result is NotFound)
        {
            return NotFound();
        }
        else if (result is ValidationError err)
        {
            return BadRequest(err.Message);
        }

        return StatusCode(500, "Internal Server Error"); // Should not happen
    }
}