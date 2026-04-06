using NSubstitute;
using AwesomeAssertions;
using Xunit;

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
    public async Task NotifyUserAsync_UserFound_SendsWelcomeEmail()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        var user = new User { Id = 1, Email = "test@example.com" };

        repo.GetByIdAsync(1).Returns(user);
        email.SendWelcomeAsync("test@example.com").Returns(Task.CompletedTask);

        var service = new NotificationService(repo, email);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        email.Received().SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(1).Returns((User?)null);

        var service = new NotificationService(repo, email);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        await service.NotifyUserAsync(1).Should().ThrowAsync<InvalidOperationException>();
    }
}