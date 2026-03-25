using Microsoft.AspNetCore.Mvc;
using OneOf;

public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetByIdAsync(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        if (result.IsNotFound())
            return NotFound();
        else if (result.IsValidationError())
            return BadRequest(result.Message);
        else
            return Ok(result.As<T0>());
    }

    private class UserRecord : Record<User>
    {
        public string Name { get; set; }
        public string Email { get; set; }
    }

    private record NotFoundRecord : Record<NotFound> { Message = "User not found" };
    private record ValidationErrorRecord : Record<ValidationError> { Message = "Validation error" };

    [GenerateOneOf]
    public partial class UserRecord
    {
        [GenerateOneOf]
        public partial class NotFoundRecord
        {
            [GenerateOneOf]
            public partial class ValidationErrorRecord
            {
                // ...
            }
        }
    }

    private record User : Record<User>
    {
        [GenerateOneOf]
        public partial class UserRecord
        {
            [GenerateOneOf]
            public partial class NotFoundRecord
            {
                [GenerateOneOf]
                public partial class ValidationErrorRecord
                {
                    // ...
                }
            }
        }
    }

    private record UserRecord
    {
        [GenerateOneOf]
        public partial class UserRecord
        {
            [GenerateOneOf]
            public partial class NotFoundRecord
            {
                [GenerateOneOf]
                public partial class ValidationErrorRecord
                {
                    // ...
                }
            }
        }
    }

    private interface IUserService
    {
        Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
    }

    private record User(string Id, string Name, string Email);

    private record NotFound(NotFoundRecord Message);

    private record ValidationError(ValidationErrorRecord Message);

    [GenerateOneOf]
    public partial class User : Record<User>
    {
        // ...
    }
}