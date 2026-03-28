using Microsoft.AspNetCore.Mvc;
using OneOf;

namespace Users.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /api/users/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);
            return result.Match(
                user => Ok(user),
                _       => NotFound(),
                err     => BadRequest(err.Message)
            );
        }
    }
}