using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// User record
public record User(int Id, string Name, string Email);

// NotFound record
public record NotFound;

// ValidationError record
public record ValidationError(string Message);

// IUserService interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// UserService implementation
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IOptions<AppSettings> _options;

    public UserService(ILogger<UserService> logger, IOptions<AppSettings> options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        // Simulate database lookup
        await Task.Delay(100);

        if (id <= 0)
        {
            return new ValidationError("Invalid user ID");
        }

        if (id == 999)
        {
            return new NotFound();
        }

        // Return a valid user
        return new User(id, $"User{id}", $"user{id}@example.com");
    }
}

// AppSettings for options pattern
public class AppSettings
{
    public string ApiVersion { get; set; } = "v1";
}

// UsersController
[ApiController]
[Route("api/[controller]")]
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
            notFound => NotFound(),
            error => BadRequest(error.Message)
        );
    }
}

json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
    "ApiVersion": "v1"
  }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// User record
public record User(int Id, string Name, string Email);

// NotFound record
public record NotFound;

// ValidationError record
public record ValidationError(string Message);

// IUserService interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// UserService implementation
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IOptions<AppSettings> _options;

    public UserService(ILogger<UserService> logger, IOptions<AppSettings> options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        // Simulate database lookup
        await Task.Delay(100);

        if (id <= 0)
        {
            return new ValidationError("Invalid user ID");
        }

        if (id == 999)
        {
            return new NotFound();
        }

        // Return a valid user
        return new User(id, $"User{id}", $"user{id}@example.com");
    }
}

// AppSettings for options pattern
public class AppSettings
{
    public string ApiVersion { get; set; } = "v1";
}

// UsersController
[ApiController]
[Route("api/[controller]")]
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
            notFound => NotFound(),
            error => BadRequest(error.Message)
        );
    }
}

json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
    "ApiVersion": "v1"
  }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// User record
public record User(int Id, string Name, string Email);

// NotFound record
public record NotFound;

// ValidationError record
public record ValidationError(string Message);

// IUserService interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// UserService implementation
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IOptions<AppSettings> _options;

    public UserService(ILogger<UserService> logger, IOptions<AppSettings> options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        // Simulate database lookup
        await Task.Delay(100);

        if (id <= 0)
        {
            return new ValidationError("Invalid user ID");
        }

        if (id == 999)
        {
            return new NotFound();
        }

        // Return a valid user
        return new User(id, $"User{id}", $"user{id}@example.com");
    }
}

// AppSettings for options pattern
public class AppSettings
{
    public string ApiVersion { get; set; } = "v1";
}

// UsersController
[ApiController]
[Route("api/[controller]")]
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
            notFound => NotFound(),
            error => BadRequest(error.Message)
        );
    }
}

json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
    "ApiVersion": "v1"
  }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

// User record
public record User(int Id, string Name, string Email);

// NotFound record
public record NotFound;

// ValidationError record
public record ValidationError(string Message);

// IUserService interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// UserService implementation
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IOptions<AppSettings> _options;

    public UserService(ILogger<UserService> logger, IOptions<AppSettings> options)
    {
        _logger = logger;
        _options = options;
    }

    public async Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        // Simulate database lookup
        await Task.Delay(100);

        if (id <= 0)
        {
            return new ValidationError("Invalid user ID");
        }

        if (id == 999)
        {
            return new NotFound();
        }

        // Return a valid user
        return new User(id, $"User{id}", $"user{id}@example.com");
    }
}

// AppSettings for options pattern
public class AppSettings
{
    public string ApiVersion { get; set; } = "v1";
}

// UsersController
[ApiController]
[Route("api/[controller]")]
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
            notFound => NotFound(),
            error => BadRequest(error.Message)
        );
    }
}

json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AppSettings": {
    "ApiVersion": "v1"
  }
}