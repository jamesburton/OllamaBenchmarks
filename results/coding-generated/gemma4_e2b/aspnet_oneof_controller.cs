using Microsoft.AspNetCore.Mvc;
using OneOf;

// --- Domain/Result Types ---

// User record (Class for object initializer support)
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// NotFound record
public record NotFound;

// ValidationError record
public record ValidationError(string Message);

// --- Service Interface ---

public interface IUserService
{
    OneOf<User, NotFound, ValidationError> GetByIdAsync(int id);
}

// --- Example Service Implementation (Mocked for completeness) ---

public class UserService : IUserService
{
    public OneOf<User, NotFound, ValidationError> GetByIdAsync(int id)
    {
        if (id == 1)
        {
            return new User { Id = 1, Name = "Alice", Email = "a@b.com" };
        }
        if (id == 99)
        {
            return new NotFound();
        }
        return new ValidationError("User not found");
    }
}

// --- Controller ---

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retrieves a user by ID, handling success, not found, and validation errors using OneOf.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        // Call the service which returns the discriminated union
        OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);

        // Use the Match method to map the result to the appropriate IActionResult
        return result.Match<IActionResult>(
            user    => Ok(user),
            notFound => NotFound(),
            err     => BadRequest(new { Message = err.Message }) // Map ValidationError to 400 Bad Request
        );
    }
}