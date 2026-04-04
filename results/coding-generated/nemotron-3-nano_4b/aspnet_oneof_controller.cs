using OneOf;

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public record NotFound;

public record ValidationError(string Message);

public class UserService : IUserService
{
    public Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        if (id <= 0)
            return Task.FromResult(OneOf<NotFound, ValidationError>.FromT0(new ValidationError("Invalid user ID")));
        return Task.FromResult(OneOf<User, NotFound, ValidationError>.FromT0(new User { Id = id, Name = "User", Email = "user@example.com" }));
    }
}

public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    public UsersController(IUserService service) => _service = service;
    public async Task<IActionResult> Get(int id)
    {
        var result = _service.GetByIdAsync(id);
        return result.Match<IActionResult>(
            u => Ok(u),
            _ => Results.NotFound(),
            ve => Results.BadRequest(ve.Message));
    }
}