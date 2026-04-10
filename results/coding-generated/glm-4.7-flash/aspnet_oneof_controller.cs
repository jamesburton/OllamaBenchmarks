using OneOf;
using Microsoft.AspNetCore.Mvc;

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

public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Get(int id)
    {
        return await _service.GetByIdAsync(id).Match<IActionResult>(
            user => Ok(user),
            notFound => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}