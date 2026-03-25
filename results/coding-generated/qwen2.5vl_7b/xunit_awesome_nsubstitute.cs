using xunit.v3;
using NSubstitute;
using AwesomeAssertions;

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
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        var user = new User { Id = 1, Email = "user@example.com" };
        userRepository.GetByIdAsync(1).Returns(Task.FromResult(user));
        emailService.SendWelcomeAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var notificationService = new NotificationService(userRepository, emailService);

        // Act
        await notificationService.NotifyUserAsync(1);

        // Assert
        userRepository.Received(1)..GetByIdAsync(1);
        emailService.Received(1).SendWelcomeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        userRepository.GetByIdAsync(1).Returns(Task.FromResult<User?>(null));

        var notificationService = new NotificationService(userRepository, emailService);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => notificationService.NotifyUserAsync(1));
    }

    [Fact]
    public async Task NotifyUserAsync_EmailSentCorrectly()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        var user = new User { Id = 1, Email = "user@example.com" };
        userRepository.GetByIdAsync(1).Returns(Task.FromResult(user));
        emailService.SendWelcomeAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var notificationService = new NotificationService(userRepository, emailService);

        // Act
        await notificationService.NotifyUserAsync(1);

        // Assert
        userRepository.Received(1)..GetByIdAsync(1);
        emailService.Received(1).SendWelcomeAsync(Arg.Any<string>());
    }
}