using OneOf;

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

public class UsersController
{
    private readonly IUserService _userService;
    public UsersController(IUserService userService) => _userService = userService;

    public async Task<IActionResult> Get(int id)
    {
        OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);

        if (result.TryPickT0(out User user, out var remainder))
            return Ok(user);

        if (result.TryPickT1(out NotFound notFound, out var remainder2))
            return NotFound();

        return Results.BadRequest(remainder2.Message);
    }
}