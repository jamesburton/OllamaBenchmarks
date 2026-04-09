using Microsoft.AspNetCore.Mvc;
using OneOf;
using System.Threading.Tasks;

public record NotFound;
public record ValidationError(string Message);

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Get(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Match(
            user => Results.Ok(user),
            _ => Results.NotFound(),
            err => Results.BadRequest(err.Message)
        );
    }
}