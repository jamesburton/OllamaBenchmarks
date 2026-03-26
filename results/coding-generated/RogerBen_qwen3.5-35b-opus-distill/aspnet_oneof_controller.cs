using OneOf;
using Microsoft.AspNetCore.Mvc;

namespace UsersApi;

// Service interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// User record
public record User(int Id, string Name, string Email);

// NotFound record
public record NotFound(string Message = "User not found");

// ValidationError record
public record ValidationError(string Message);

// Controller
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user    => Ok(user),
            notFound => NotFound(),
            err     => BadRequest(err.Message)
        );
    }
}