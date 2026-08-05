using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace UsersController
{
    [ApiController]
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

            if (result.IsT0)
            {
                return Ok(result.AsT0);
            }
            else if (result.IsT1)
            {
                return NotFound();
            }
            else
            {
                return BadRequest(result.AsT2);
            }
        }

        private class User : IEquatable<User>
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }

            public bool Equals(User other)
            {
                if (other == null) return false;
                return this.Id == other.Id && this.Name == other.Name && this.Email == other.Email;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((this.Id.GetHashCode() * 397) ^ this.Name.GetHashCode()) ^
                           this.Email.GetHashCode();
                }
            }
        }

        private class NotFound : IEquatable<NotFound>
        {
            public NotFound() { }

            public bool Equals(NotFound other)
            {
                if (other == null) return false;
                return true;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((0 * 397) ^ this.GetType().Name.GetHashCode());
                }
            }
        }

        private class ValidationError : IEquatable<ValidationError>
        {
            public string Message { get; set; }

            public bool Equals(ValidationError other)
            {
                if (other == null) return false;
                return this.Message.Equals(other.Message);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((this.Message.GetHashCode() * 397) ^ this.GetType().Name.GetHashCode());
                }
            }
        }
    }
}