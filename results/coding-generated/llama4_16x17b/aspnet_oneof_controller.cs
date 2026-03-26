using Microsoft.AspNetCore.Mvc;
using OneOf;

public record User(int Id, string Name, string Email);

public record NotFound;

public record ValidationError(string Message);

public interface IUserService
{
    OneOf<User, NotFound, ValidationError> GetByIdAsync(int id);
}

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