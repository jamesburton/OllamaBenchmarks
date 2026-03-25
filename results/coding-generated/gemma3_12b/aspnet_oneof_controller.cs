using Microsoft.AspNetCore.Mvc;
using OneOf;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

public record User(int Id, string Name, string Email);

public record NotFound();

public record ValidationError(string Message);

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService, IOptions<SmtpOptions> smtpOptions)
{
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        OneOf<User, NotFound, ValidationError> result = await userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Ok(user),
            _ => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}

public class SmtpOptions
{
    public string Host { get; set; }
    public int Port { get; set; }
}