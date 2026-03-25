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

using NSubstitute;
using AwesomeAssertions;
using Xunit;
using System.Threading.Tasks;

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUserAsync_HappyPath()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        userRepository.GetByIdAsync(1).Returns(user);

        var notificationService = new NotificationService(userRepository, emailService);

        // Act
        await notificationService.NotifyUserAsync(1);

        // Assert
        await userRepository.Received().GetByIdAsync(1);
        emailService.Received().SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        userRepository.GetByIdAsync(1).Returns((User?)null);

        var notificationService = new NotificationService(userRepository, emailService);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => notificationService.NotifyUserAsync(1));

        // Assert
        exception.Message.Should().Be("User not found");
        await userRepository.Received().GetByIdAsync(1);
        emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}