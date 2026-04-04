public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task SaveAsync(User user);
}

public interface IEmailService
{
    Task SendWelcomeAsync(string email);
}

public record User(int Id, string Name = "", string Email = "");

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

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUserAsync_UserFound_SendsWelcomeEmailWithCorrectEmail()
    {
        // Arrange
        var userId = 1;
        var expectedEmail = "user@example.com";
        var user = new User(userId, "", expectedEmail);

        var emailService = Substitute.For<IEmailService>();
        emailService.SendWelcomeAsync(expectedEmail).Returns(Task.CompletedTask);

        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(userId).ReturnsAsync(user);

        var notificationService = new NotificationService(repo, emailService);

        // Act & Assert
        await notificationService.NotifyUserAsync(userId);

        emailService.Received().SendWelcomeAsync(expectedEmail);
        notificationService.Received();
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 999;
        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(userId).ReturnsAsync((User?)null);

        var emailService = Substitute.For<IEmailService>();

        var notificationService = new NotificationService(repo, emailService);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => notificationService.NotifyUserAsync(userId));
        notificationService.Received().GetByIdAsync(userId);
        notificationService.Received().Throw();
    }
}