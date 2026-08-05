using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// --- OneOf Implementation (Simulated based on API reference) ---
// In a real project, this would be imported via the NuGet package.
public class OneOf<T0, T1, T2>
{
    private readonly T0 _t0;
    private readonly T1 _t1;
    private readonly T2 _t2;

    public OneOf(T0 t0, T1 t1, T2 t2)
    {
        _t0 = t0;
        _t1 = t1;
        _t2 = t2;
    }

    public static OneOf<T0, T1, T2> FromT0(T0 t0) => new OneOf<T0, T1, T2>(t0, default, default);
    public static OneOf<T0, T1, T2> FromT1(T1 t1) => new OneOf<T0, T1, T2>(default, t1, default);

    public bool IsT0 => _t0 != null;
    public bool IsT1 => _t1 != null;
    public bool IsT2 => _t2 != null;

    // Match implementation
    public TResult Match<TResult>(Func<T0, TResult> t0, Func<T1, TResult> t1, Func<T2, TResult> t2)
    {
        if (IsT0) return t0(_t0);
        if (IsT1) return t1(_t1);
        if (IsT2) return t2(_t2);
        throw new InvalidOperationException("OneOf result was null or unhandled.");
    }

    // Switch implementation (simplified for this example)
    public void Switch(Func<T0, object> t0, Func<T1, object> t1, Func<T2, object> t2)
    {
        if (IsT0) t0(_t0);
        else if (IsT1) t1(_t1);
        else if (IsT2) t2(_t2);
    }

    // Type Check / Value Access
    public bool IsT0 { get; }
    public T0 AsT0 => _t0;
    public bool TryPickT0(out T0 value, out OneOf<T1, T2> remainder)
    {
        if (IsT0)
        {
            value = _t0;
            remainder = new OneOf<T1, T2>(default, _t1, _t2);
            return true;
        }
        // Simplified: In a real implementation, this would handle the remainder correctly.
        return false;
    }
}

// --- Data Models ---

// User must be a class supporting object initializer syntax
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// NotFound must be a record
public record NotFound;

// ValidationError must be a record
public record ValidationError(string Message);

// --- Service Interface and Implementation ---

public interface IUserService
{
    OneOf<User, NotFound, ValidationError> GetByIdAsync(int id);
}

public class UserService : IUserService
{
    public async Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id)
    {
        if (id == 1)
        {
            var user = new User { Id = 1, Name = "Alice", Email = "a@b.com" };
            return OneOf<User, NotFound, ValidationError>.FromT0(user);
        }
        if (id == 404)
        {
            return OneOf<User, NotFound, ValidationError>.FromT1(new NotFound());
        }
        if (id == 400)
        {
            return OneOf<User, NotFound, ValidationError>.FromT2(new ValidationError("User not found for ID: " + id));
        }

        // Default case
        return OneOf<User, NotFound, ValidationError>.FromT1(new NotFound());
    }
}

// --- Controller ---

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retrieves a user by ID, handling success, not found, and validation errors.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>The user object, 404 Not Found, or 400 Bad Request.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        // 1. Call the service, which returns the OneOf union type
        var result = await _userService.GetByIdAsync(id);

        // 2. Use Match to handle the different types exhaustively and map them to IActionResult
        return result.Match<IActionResult>(
            user    => Ok(user),
            notFound => NotFound(),
            err     => BadRequest(new { Message = err.Message }) // Map ValidationError to 400 Bad Request
        );
    }
}

// --- Program Setup (Minimal Host Context for completeness) ---
/*
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddControllers();
        var app = builder.Build();

        app.MapControllers();
        app.Run();
    }
}
*/