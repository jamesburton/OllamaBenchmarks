using Microsoft.AspNetCore.Mvc;
using OneOf;
using System;
using System.Threading.Tasks;

// --- Domain Models ---

// User must be a class with public get/set properties
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// NotFound must be a record with a parameterless constructor
public record NotFound;

// ValidationError must be a record accepting a single string Message parameter
public record ValidationError(string Message);

// --- Service Interface ---

public interface IUserService
{
    // The service returns the OneOf discriminated union
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// --- Controller ---

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // Action method must be named 'Get'
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        // Call the service which returns the OneOf union
        var result = await _userService.GetByIdAsync(id);

        // Use the Match pattern to handle the discriminated union result
        return result.Match<IActionResult>(
            user => Ok(user), // T0: User -> 200 OK
            _ => NotFound(),  // T1: NotFound -> 404 Not Found
            err => BadRequest(err.Message) // T2: ValidationError -> 400 Bad Request
        );
    }
}

// --- Mock Implementation for Compilation/Testing ---
// In a real application, this would be registered via Program.cs/Startup.cs
public class MockUserService : IUserService
{
    public Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        if (id == 1)
        {
            var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(user));
        }
        else if (id == 99)
        {
            // Simulate validation error
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new ValidationError("User ID must be positive.")));
        }
        else
        {
            // Simulate not found
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new NotFound()));
        }
    }
}

// Note: For this code to run, the project must have the OneOf NuGet package installed.