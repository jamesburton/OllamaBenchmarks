using System.Net;
using Microsoft.AspNetCore.Mvc;

// Top-level types (Global Namespace)

[ApiController]
[Route("api/[controller]/users")] // Route: /api/users/{id} based on controller name and route template logic, adjusted to match requirement "GET /api/users/{id}"
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // Action named 'Get' as required
    [HttpGet("{id}")] 
    public async Task<IActionResult> Get(int id)
    {
        OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);

        return result.Match(
            user => Ok(user),
            notFound => StatusCode((int)HttpStatusCode.NotFound), // 404
            err => BadRequest(err.Message)                        // 400 with message
        );
    }
}

// Record for NotFound (parameterless constructor required)
public record NotFound;

// Record for ValidationError (single string Message parameter)
public record ValidationError(string Message);

// Class for User (not a positional record, supports object initializer syntax)
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}

// Interface for the service
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}