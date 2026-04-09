using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Results;
using OneOf;
using System.Threading.Tasks;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public record NotFound;

public record ValidationError(string Message);

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

[ApiController]
[Route("api/users")]
public class UsersController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Results.Ok(user),
            _ => Results.NotFound(),
            err => Results.BadRequest(err.Message)
        );
    }
}