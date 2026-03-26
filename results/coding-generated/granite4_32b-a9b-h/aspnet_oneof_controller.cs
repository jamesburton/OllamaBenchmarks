using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneOf;

// User record with public get/set properties for object-initializer syntax
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// NotFound and ValidationError records as specified
public record NotFound;
public record ValidationError(string Message);

// IUserService interface used by the controller
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// UsersController implementing the required endpoint
[ApiController]
[Route("api/[controller]")]
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
        var result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Ok(user),
            notFound => NotFound(),
            error => BadRequest(error.Message)
        );
    }
}