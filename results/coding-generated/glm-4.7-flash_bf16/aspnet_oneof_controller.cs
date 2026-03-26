using OneOf;

namespace MyProject.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
   枢

    public record NotFound { }

    public record ValidationError(string Message) { }
    // ================================================

    // Services/IUserService.cs
    using OneOf;

    namespace MyProject.Services
    {
        public interface IUserService
        {
            Task<OneOf<User, NotFound, ValidationError>> GetByIdAsync(int id);
        }
说了
        // ================================================

        // Controllers/UsersController.cs
        using Microsoft.AspNetCore.Mvc;
        using OneOf;
        using MyProject.Models;
        using MyProject.Services;

        [Route("api/[controller]")]
        [ApiController]
        public class UsersController : ControllerBase
        {
            private readonly IUserService _userService;

            public UsersController(IUserService userService)
            {
                _userService = userService;
            }

            [HttpGet("{id}")]
            public async Task<OneOf<OkResult, NotFoundResult, BadRequestObjectResult>> Get(int id)
            {
                var result = await _userService.GetByIdAsync(id);

                return result.Match(
                    user => Ok(user),
                    _ => NotFound(),
                    error => BadRequest(error.Message)
                );
            }
        }
    }
}