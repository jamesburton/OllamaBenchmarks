using Microsoft.AspNetCore.Mvc;
using OneOf;

// --- Data Structures ---

// User class (must support object-initializer syntax)
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

// --- Example Service Implementation (for context, not strictly required by prompt) ---
// In a real application, this would be injected.
public class UserService : IUserService
{
    public OneOf<User, NotFound, ValidationError> GetByIdAsync(int id)
    {
        if (id == 1)
        {
            return new User { Id = 1, Name = "Alice", Email = "a@b.com" };
        }
        if (id == 404)
        {
            return new NotFound();
        }
        if (id == 400)
        {
            return new ValidationError("User not found for ID: " + id);
        }
        // Default case if needed
        return new NotFound();
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
    /// Retrieves a user by ID, handling success, not found, and validation errors.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>The appropriate HTTP result (200, 404, or 400).</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        // Call the service which returns the OneOf union type
        OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);

        // Use the Match method to map the union type to the appropriate IActionResult
        return result.Match<IActionResult>(
            user    => Ok(user),             // Maps User to 200 OK
            notFound => NotFound(),           // Maps NotFound to 404 Not Found
            err     => BadRequest(new
                {
                    Message = err.Message,
                    Status = 400
                }) // Maps ValidationError to 400 Bad Request
        );
    }
}