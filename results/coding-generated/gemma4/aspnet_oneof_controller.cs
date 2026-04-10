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

// NotFound record type (parameterless constructor)
public record NotFound;

// ValidationError record type (accepts Message)
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
    public async Task<IActionResult> Get(int id)
    {
        // 1. Call the service layer
        var result = await _userService.GetByIdAsync(id);

        // 2. Use Match to handle the discriminated union and map to IActionResult
        return result.Match<IActionResult>(
            // Case 1: Success (User) -> 200 OK
            user => Ok(user),

            // Case 2: Not Found (NotFound) -> 404 Not Found
            // Note: We use the default record instance here.
            _ => NotFound(), 

            // Case 3: Validation Error (ValidationError) -> 400 Bad Request
            // We must cast the remainder to ValidationError to access the message.
            err => BadRequest(err.Message)
        );
    }
}

// --- Mock Service Implementation (for completeness/testing) ---

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
            // Validation Error case
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new ValidationError("User ID must be positive.")));
        }
        else if (id == 0)
        {
            // Not Found case
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new NotFound()));
        }
        else
        {
            // Default fallback (e.g., if the service logic changes)
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new NotFound()));
        }
    }
}

// --- Program Setup (Simulating Dependency Injection) ---
// This section is not required for the final output but shows how the components fit together.
/*
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register services
        builder.Services.AddSingleton<IUserService, MockUserService>();
        builder.Services.AddControllers();

        var app = builder.Build();
        app.MapControllers();
        app.Run();
    }
}
*/