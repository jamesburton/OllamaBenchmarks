using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VerticalSlice.Api
{
    // --- Records ---

    public record CreateUserRequest(
        [property: MinLength(3)] string Username,
        [property: EmailAddress] string Email,
        [property: MinLength(8)] string Password
    );

    public record UserDto(Guid Id, string Username, string Email, DateTime CreatedAt);

    // --- Service Interface ---

    public interface IUserService
    {
        Task<UserDto> CreateAsync(CreateUserRequest req, CancellationToken ct);
        Task<UserDto?> GetAsync(Guid id, CancellationToken ct);
        Task<IReadOnlyList<UserDto>> ListAsync(int page, int pageSize, CancellationToken ct);
    }

    // --- Service Implementation ---

    public class InMemoryUserService : IUserService
    {
        private readonly List<UserDto> _users = new();

        public Task<UserDto> CreateAsync(CreateUserRequest req, CancellationToken ct)
        {
            var user = new UserDto(
                Guid.NewGuid(),
                req.Username,
                req.Email,
                DateTime.UtcNow
            );

            _users.Add(user);
            return Task.FromResult(user);
        }

        public Task<UserDto?> GetAsync(Guid id, CancellationToken ct)
        {
            var user = _users.FirstOrDefault(u => u.Id == id);
            return Task.FromResult(user);
        }

        public Task<IReadOnlyList<UserDto>> ListAsync(int page, int pageSize, CancellationToken ct)
        {
            var result = _users
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult<IReadOnlyList<UserDto>>(result);
        }
    }

    // --- Controller ---

    [ApiController]
    [Route("api/v1/users")]
    public class UsersV1Controller : ControllerBase
    {
        private readonly IUserService _svc;

        public UsersV1Controller(IUserService svc)
        {
            _svc = svc;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest req, CancellationToken ct)
        {
            var user = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken ct)
        {
            var user = await _svc.GetAsync(id, ct);
            if (user is null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
        {
            var users = await _svc.ListAsync(page, pageSize, ct);
            return Ok(users);
        }
    }
}