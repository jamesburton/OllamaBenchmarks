using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// Global types
public record User(int Id, string Name, string Email);
public record NotFound;
public record ValidationError(string Message);

// Service interface
public interface IUserService
{
    Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
}

// OneOf implementation (simplified for example)
public class OneOf<T1, T2, T3>
{
    public T1 Value { get; }
    public T2 Value2 { get; }
    public T3 Value3 { get; }

    private OneOf(T1 v, T2 v2, T3 v3) => (Value, Value2, Value3) = (v, v2, v3);

    public OneOf(T1 v) => new OneOf<T1, T2, T3>(v, default, default);
    public OneOf(T2 v) => new OneOf<T1, T2, T3>(default, v, default);
    public OneOf(T3 v) => new OneOf<T1, T2, T3>(default, default, v);

    public bool IsT0 => EqualityComparer<T1>.Default.Equals(Value, default);
    public bool IsT1 => EqualityComparer<T2>.Default.Equals(Value2, default);
    public bool IsT2 => EqualityComparer<T3>.Default.Equals(Value3, default);

    public T1 AsT0 => IsT0 ? Value : throw new InvalidOperationException();
    public T2 AsT1 => IsT1 ? Value2 : throw new InvalidOperationException();
    public T3 AsT2 => IsT2 ? Value3 : throw new InvalidOperationException();

    public T1 TryPickT0(out T1 user, out T2 _1, out T3 _2)
    {
        if (IsT0)
        {
            user = Value;
            _1 = default;
            _2 = default;
            return true;
        }
        user = default;
        _1 = default;
        _2 = default;
        return false;
    }

    public T1 Match(Func<T1, T1> f, T2 action, T3 error)
    {
        return IsT0 ? f(Value) : IsT1 ? action(Value2) : error;
    }

    public void Switch(Action<T1> onT1, Action<T2> onT2, Func<T3, T1> onError)
    {
        if (IsT0) onT1(Value);
        else if (IsT1) onT2(Value2);
        else onError(Value3);
    }
}

// Controller
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return result.Match(
            user => Ok(user),
            notFound => NotFound(),
            err => BadRequest(err.Message)
        );
    }
}