public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task SaveAsync(User user);
    }

    public interface IEmailService
    {
        Task SendWelcomeAsync(string email);
    }

    public record User
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public string Email { get; init; } = "";
    }

    public class NotificationService(IUserRepository repo, IEmailService email)
    {
        public async Task NotifyUserAsync(int userId)
        {
            var user = await repo.GetByIdAsync(userId)
                ?? throw new InvalidOperationException("User not found");
            await email.SendWelcomeAsync(user.Email);
        }
    }

    public class NotificationServiceTests
    {
        [Fact]
        public async Task NotifyUserAsync_HappyPath()
        {
            var repo = Substitute.For<IUserRepository>();
            var email = Substitute.For<IEmailService>();

            var user = new User { Id = 1, Email = "test@example.com" };
            repo.GetByIdAsync(1).Returns(Task.FromResult(user));
            email.SendWelcomeAsync("test@example.com").Returns(Task.CompletedTask);

            var sut = new NotificationService(repo, email);

            await sut.NotifyUserAsync(1);

            // Verify calls
            email.Received().SendWelcomeAsync("test@example.com");
            repo.Received().GetByIdAsync(1);
        }

        [Fact]
        public async Task NotifyUserAsync_UserNotFound_Throws()
        {
            var repo = Substitute.For<IUserRepository>();
            var email = Substitute.For<IEmailService>();

            repo.GetByIdAsync(99).Returns(Task.FromResult((User?)null));

            var sut = new NotificationService(repo, email);

            await sut.NotifyUserAsync(99);
            await sut.Should().ThrowAsync<InvalidOperationException>();
        }
    }

var act = sut.NotifyUserAsync(99);
    await act.Should().ThrowAsync<InvalidOperationException>();

var act = sut.NotifyUserAsync(99);
    await act.Should().ThrowAsync<InvalidOperationException>();

var act = sut.NotifyUserAsync(99);
    await act.Should().ThrowAsync<InvalidOperationException>();

await sut.NotifyUserAsync(1);
    email.Received().SendWelcomeAsync("test@example.com");
    repo.Received().GetByIdAsync(1);

Assert.Multiple(
        () => Assert.Equal(1, a),
        () => Assert.Equal(2, b)
    );