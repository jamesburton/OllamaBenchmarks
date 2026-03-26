using System;
using System.Threading.Tasks;
using OneOf;
using OneOf.Types;

// Global namespace for simplicity as per instructions

[GenerateOneOf]
public partial class GetUserResult : OneOf<User, NotFound, ValidationError> { }

public interface IUserService
{
    Task<GetUserResult> GetByIdAsync(int id);
}

public class UserService : IUserService
{
    public async Task<GetUserResult> GetByIdAsync(int id)
    {
        // Simulated user retrieval logic
        if (id == 1)
            return OneOf<User, NotFound, ValidationError>.FromT0(new User { Id = 1, Name = "Alice", Email = "alice@example.com" });

        if (id < 0)
            return OneOf<User, NotFound, ValidationError>.FromT2(new ValidationError("Invalid user ID"));

        return OneOf<User, NotFound, ValidationError>.FromT1(new NotFound());
    }
}

public class UsersController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Ok(user),
            notFound => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}

public record User
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
}

public record NotFound();

public record ValidationError(string Message);