using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneOf;

// Domain models
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}

public record NotFound;

public record ValidationError(string Message);

// Service contract
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) =>
        _userService = userService;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Ok(user),
            notFound => base.NotFound(),
            validationError => BadRequest(validationError.Message)
        );
    }
}