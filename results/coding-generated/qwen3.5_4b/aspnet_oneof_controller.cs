using OneOf;
using Microsoft.AspNetCore.Mvc;

// User record as a class to support object-initializer syntax
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Record type for NotFound (parameterless constructor)
public record NotFound;

// Record type for ValidationError (single string parameter)
public record ValidationError(string Message);

// Service Interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// Minimal implementation of the service
public class UserService : IUserService
{
    public async Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        // Simulated logic
        if (id == 1)
        {
            return OneOf<User, NotFound, ValidationError>.FromT0(new User { Id = 1, Name = "Alice", Email = "a@b.com" });
        }
        else if (id == 2)
        {
            return OneOf<User, NotFound, ValidationError>.FromT1(new NotFound());
        }
        else
        {
            return OneOf<User, NotFound, ValidationError>.FromT2(new ValidationError($"User {id} not found in database"));
        }
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class UsersController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Get(int id)
    {
        OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user    => Ok(user),
            _       => NotFound(),
            err     => BadRequest(err.Message)
        );
    }
}