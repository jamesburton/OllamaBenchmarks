using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.AspNetCore;

namespace YourNamespace.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Ok(user),
            _ => NotFound(),
            error => BadRequest(error.Message)
        );
    }
}

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

[GenerateOneOf]
public partial class GetUserResult : OneOf<User, NotFound, ValidationError> { }

public record User(int Id, string Name, string Email);

public record NotFound();

public record ValidationError(string Message);