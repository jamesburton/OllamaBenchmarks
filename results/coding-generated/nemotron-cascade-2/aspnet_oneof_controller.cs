using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace SampleApi;

/// <summary>Domain model returned from the service.</summary>
public record User(int Id, string Name, string Email);

/// <summary>Indicates the requested resource was not found.</summary>
public record NotFound;

/// <summary>Wraps a validation error message.</summary>
public record ValidationError(string Message);

/// <summary>Service contract for retrieving a user.</summary>
public interface IUserService
{
    /// <summary>Gets a user by its identifier.</summary>
    /// <param name="id">User identifier.</param>
    /// <returns>
    /// A discriminated union containing either a <see cref="User"/>, a <see cref="NotFound"/> result,
    /// or a <see cref="ValidationError"/>.
    /// </returns>
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

/// <summary>API controller for user operations.</summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service) => _service = service;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        OneOf<User, NotFound, ValidationError> result = await _service.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Ok(user),
            notFound => NotFound(),
            validationError => BadRequest(validationError.Message));
    }
}