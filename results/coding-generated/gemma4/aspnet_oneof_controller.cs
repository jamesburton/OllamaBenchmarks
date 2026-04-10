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

    // GET /api/users/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        // 1. Call the service which returns the OneOf union
        var result = await _userService.GetByIdAsync(id);

        // 2. Use Match to handle the discriminated union and map to appropriate HTTP results
        return result.Match<IActionResult>(
            // Case 1: User found (200 OK)
            user => Ok(user),

            // Case 2: Not found (404 Not Found)
            // Note: We use the default record instance here
            _ => NotFound(), 

            // Case 3: Validation error (400 Bad Request)
            // The 'err' parameter captures the ValidationError instance
            err => BadRequest(new { Error = err.Message })
        );
    }
}

// --- Mock Implementation for Demonstration ---
// In a real application, this would be registered via services.AddScoped
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
            // Validation error case
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new ValidationError("The provided ID format is invalid.")));
        }
        else if (id == 0)
        {
            // Not found case
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new NotFound()));
        }
        else
        {
            // Defaulting to Not Found for other IDs
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new NotFound()));
        }
    }
}

// Example of how DI would register the mock service (optional, but good practice)
/*
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMockServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, MockUserService>();
        return services;
    }
}
*/