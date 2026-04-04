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

// NotFound must be a record with a parameterless constructor
public record NotFound;

// ValidationError must be a record accepting a single string Message parameter
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
        var result = await _userService.GetByIdAsync(id);

        // 2. Use Match to handle the discriminated union and map to appropriate HTTP results
        return result.Match<IActionResult>(
            // Case 1: User found (200 OK)
            user => Ok(user),

            // Case 2: Not Found (404 Not Found)
            // Note: We use the default record instance here
            _ => NotFound(), 

            // Case 3: Validation Error (400 Bad Request)
            // The 'err' parameter is the ValidationError record
            err => BadRequest(new { Error = err.Message })
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
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new ValidationError("The provided ID format is invalid.")));
        }
        else if (id == 0)
        {
            // Not Found case
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new NotFound()));
        }
        else
        {
            // Defaulting to Not Found for other IDs
            return Task.FromResult(new OneOf<User, NotFound, ValidationError>(new NotFound()));
        }
    }
}

// --- Program Setup (Simulating Dependency Injection) ---
/*
// In a real ASP.NET Core 10 setup, you would register these services:
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddSingleton<IUserService, MockUserService>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();
*/