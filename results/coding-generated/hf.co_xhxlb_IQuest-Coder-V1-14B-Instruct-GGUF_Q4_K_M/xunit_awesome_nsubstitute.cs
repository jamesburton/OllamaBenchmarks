using Xunit;
using AwesomeAssertions;
using NSubstitute;

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
    public async Task NotifyUserAsync_WhenUserExists_SendsWelcomeEmail()
    {
        // Arrange
        var userId = 1;
        var expectedEmail = "test@example.com";
        var user = new User { Id = userId, Name = "Test User", Email = expectedEmail };

        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(userId).Returns(user);

        var emailService = Substitute.For<IEmailService>();

        var service = new NotificationService(repo, emailService);

        // Act
        await service.NotifyUserAsync(userId);

        // Assert
        repo.Received().GetByIdAsync(userId);
        emailService.Received().SendWelcomeAsync(expectedEmail);
    }

    [Fact]
    public async Task NotifyUserAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 999;

        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(userId).Returns((User?)null);

        var emailService = Substitute.For<IEmailService>();

        var service = new NotificationService(repo, emailService);

        // Act & Assert
        var act = async () => await service.NotifyUserAsync(userId);
        await act.Should().ThrowAsync<InvalidOperationException>();

        repo.Received().GetByIdAsync(userId);
        emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task NotifyUserAsync_WithMultipleUsers_SendsEmailToCorrectUser()
    {
        // Arrange
        var userId1 = 1;
        var userId2 = 2;
        var user1 = new User { Id = userId1, Name = "User 1", Email = "user1@example.com" };
        var user2 = new User { Id = userId2, Name = "User 2", Email = "user2@example.com" };

        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(userId1).Returns(user1);
        repo.GetByIdAsync(userId2).Returns(user2);

        var emailService = Substitute.For<IEmailService>();

        var service = new NotificationService(repo, emailService);

        // Act
        await service.NotifyUserAsync(userId1);
        await service.NotifyUserAsync(userId2);

        // Assert
        repo.Received(2).GetByIdAsync(Arg.Any<int>());
        emailService.Received(2).SendWelcomeAsync(Arg.Any<string>());
        emailService.Received().SendWelcomeAsync("user1@example.com");
        emailService.Received().SendWelcomeAsync("user2@example.com");
    }
}