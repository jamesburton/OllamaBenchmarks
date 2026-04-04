using OneOf;
using Microsoft.AspNetCore.Mvc;

public record NotFound;

public record ValidationError(string Message);

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

public class User : IEquatable<User>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public bool Equals(User other) => other != null && Id == other.Id;
    public override bool Equals(object? obj) => obj is User user && Equals(user);
    public override int GetHashCode() => Id.GetHashCode();
}

public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Ok(user),
            _ => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}