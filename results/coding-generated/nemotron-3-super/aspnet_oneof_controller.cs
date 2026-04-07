using Microsoft.AspNetCore.Mvc;
using OneOf;
using System.Threading.Tasks;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
}

public record NotFound;
public record ValidationError(string Message);

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;
    public UsersController(IUserService service) => _service = service;

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Match(
            user => Ok(user),
            notFound => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}