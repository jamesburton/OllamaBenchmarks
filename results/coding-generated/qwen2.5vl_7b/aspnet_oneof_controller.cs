using Microsoft.AspNetCore.Mvc;
using OneOf;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

// Define the record types for NotFound and ValidationError
public record NotFound(string Message);
public record ValidationError(string Message);

// Define the User record
public record User(int Id, string Name, string Email);

// Define the IUserService interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// Define the UserController
[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return result.Match<IActionResult>(
            user => Ok(user),
            _ => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}

// Define the IUserService implementation
public class UserService : IUserService
{
    public async Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        // Simulate a service call
        if (id == 1)
        {
            return new User { Id = 1, Name = "John Doe", Email = "john.doe@example.com" };
        }
        else if (id == 2)
        {
            return new NotFound("User not found");
        }
        else
        {
            return new ValidationError("Invalid user ID");
        }
    }
}