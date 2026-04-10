using System;
using System.Threading.Tasks;
using OneOf;
using OneOf.Types;
using Microsoft.AspNetCore.Mvc;

public class UsersController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("/api/users/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Ok(user),
            notFound => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

public record User(int Id, string Name, string Email);

public record NotFound();

public record ValidationError(string Message);