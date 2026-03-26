using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Controllers;

public record User
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}

public record NotFound;

public record ValidationError(string Message);

public interface IUserService
{
    ValueTask<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("api/users/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Ok(user),
            notFound => NotFound(),
            validationError => BadRequest(validationError.Message)
        );
    }
}