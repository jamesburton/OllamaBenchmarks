using System;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using AwesomeAssertions;

public class NotificationServiceTests
{
    public class NotificationService
    {
        public async Task NotifyUserAsync(int userId)
        {
            var repo = Substitute.For<IUserRepository>();
            var email = Substitute.For<IEmailService>();
            var service = new NotificationService(repo, email);

            await service.NotifyUserAsync(userId);
        }
    }

    public class NotificationService
    {
        private readonly IUserRepository _repo;
        private readonly IEmailService _email;

        public NotificationService(IUserRepository repo, IEmailService email)
        {
            _repo = repo;
            _email = email;
        }

        public async Task NotifyUserAsync(int userId)
        {
            var user = await _repo.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            await _email.SendWelcomeAsync(user.Email);
        }
    }

    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
    }

    public interface IEmailService
    {
        Task SendWelcomeAsync(string email);
    }

    public record User
    {
        public int Id { get; init; }
        public string Name { get; init; }
        public string Email { get; init; }
    }
}