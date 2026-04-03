using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// --- 1. Define Custom Types using OneOf pattern ---

// User Class (supports object initializer)
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// NotFound Record
public record NotFound;

// ValidationError Record
public record ValidationError(string Message);

// --- 2. Define Service Interface ---

public interface IUserService
{
    // The service returns the discriminated union
    OneOf<User, NotFound, ValidationError> GetByIdAsync(int id);
}

// --- 3. Mock Implementation of the Service (for context) ---

public class UserService : IUserService
{
    public OneOf<User, NotFound, ValidationError> GetByIdAsync(int id)
    {
        if (id == 1)
        {
            var user = new User { Id = 1, Name = "Alice", Email = "a@b.com" };
            return user;
        }
        if (id == 99)
        {
            return new NotFound();
        }
        // Simulate a validation error for other IDs
        return new ValidationError("User not found or invalid ID.");
    }
}

// --- 4. Define the Controller ---

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
    /// Retrieves a user by ID, handling NotFound or ValidationError outcomes.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>The appropriate HTTP status code and response body.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        // Call the service which returns the OneOf union
        OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);

        // Use the Match method to map the union result to the appropriate IActionResult
        return result.Match<IActionResult>(
            user   => Ok(user),             // Maps User to 200 OK
            notFound => NotFound(),           // Maps NotFound to 404 Not Found
            err    => BadRequest(new
            {
                Message = err.Message,
                ErrorType = "Validation"
            }) // Maps ValidationError to 400 Bad Request
        );
    }
}