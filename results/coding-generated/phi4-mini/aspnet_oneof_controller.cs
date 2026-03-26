using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace YourAppNamespace.Controllers
{
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            OneOf<User, NotFound, ValidationError> result = await _userService.GetByIdAsync(id);

            return result.Match(
                User => Ok(new { Id = User.Id, Name = User.Name, Email = User.Email }),
                notFound: () => NotFound(),
                validationError: err => BadRequest(err.Message)
            );
        }
    }

    public record User(int Id, string Name, string Email);

    public record NotFound();

    public record ValidationError(string Message);

    public interface IUserService
    {
        Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
    }
}