using System;
    using Microsoft.AspNetCore.Mvc;
    using OneOf;

    public class User { ... }
    public record NotFound;
    public record ValidationError(string Message);

    public interface IUserService { Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id); }

    public class UsersController : Controller {
        private readonly IUserService _service;
        public UsersController(IUserService service) { _service = service; }
        public async Task<IActionResult> Get(int id) { ... }
    }

using System;
using Microsoft.AspNetCore.Mvc;
using OneOf;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public record NotFound;

public record ValidationError(string Message);

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

public class UsersController : Controller
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
            user => Ok(user),
            _ => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}

using System;
using Microsoft.AspNetCore.Mvc;
using OneOf;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public record NotFound;

public record ValidationError(string Message);

public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

public class UsersController : Controller
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
            user => Ok(user),
            _ => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}


... 
or 
// ... code ...