using System;
using System.Threading.Tasks;
using OneOf;
using OneOf.Types;
using YourNamespace.Services;
using YourNamespace.Models;
using YourNamespace.Exceptions;

namespace YourNamespace.Controllers
{
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("api/users/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var result = await _userService.GetByIdAsync(id);

            return result.Match<IActionResult>(
                user => Ok(user),
                notFound => NotFound(),
                error => BadRequest(error.Message)
            );
        }
    }
}