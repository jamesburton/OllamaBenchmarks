using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneOf;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
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

    public UsersController(IUserService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Get(int id)
    {
        OneOf<User, NotFound, ValidationError> result = await _service.GetByIdAsync(id);
        return result.Match<IActionResult>(
            user => Ok(user),
            _    => NotFound(),
            err  => BadRequest(err.Message)
        );
    }
}