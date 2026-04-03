using Microsoft.AspNetCore.Mvc;
using OneOf;
using System;
using System.Threading.Tasks;

// --- Model Definitions ---

// User must be a class with public get/set properties
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// NotFound record (parameterless constructor)
public record NotFound;

// ValidationError record (accepts Message)
public record ValidationError(string Message);

// --- Service Interface ---

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// --- Controller Implementation ---

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
        // 1. Call the service which returns the OneOf union
        OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);

        // 2. Use Match<IActionResult> to handle all possible types and map them to HTTP results
        return result.Match<IActionResult>(
            // Case 1: User found (200 OK)
            user => Ok(user),

            // Case 2: Not found (404 Not Found)
            notFound => NotFound(),

            // Case 3: Validation error (400 Bad Request)
            err => BadRequest(new { Error = err.Message })
        );
    }
}

// --- Mock Implementation for Testing/Completeness ---
// Note: In a real application, this would be registered in Program.cs
public class MockUserService : IUserService
{
    public Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        if (id == 1)
        {
            // Success case
            var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(user));
        }
        else if (id == 99)
        {
            // Not Found case
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new NotFound()));
        }
        else if (id == 0)
        {
            // Validation Error case
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new ValidationError("Invalid ID provided. Must be positive.")));
        }

        // Default case (e.g., if the service logic was more complex)
        return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new NotFound()));
    }
}