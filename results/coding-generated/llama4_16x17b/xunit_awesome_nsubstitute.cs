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
    public async Task NotifyUserAsync_HappyPath_SendEmail()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var userId = 1;
        var user = new User { Id = userId, Email = "test@example.com" };
        repo.GetByIdAsync(userId).Returns(user);

        var service = new NotificationService(repo, email);

        // Act
        await service.NotifyUserAsync(userId);

        // Assert
        await email.Received(1).SendWelcomeAsync(user.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var userId = 1;
        repo.GetByIdAsync(userId).Returns((User?)null);

        var service = new NotificationService(repo, email);

        // Act and Assert
        await service.Invoking(s => s.NotifyUserAsync(userId))
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task NotifyUserAsync_GetUserFails_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var userId = 1;
        repo.GetByIdAsync(userId).Returns(Task.FromResult((User?)null));

        var service = new NotificationService(repo, email);

        // Act and Assert
        await service.Invoking(s => s.NotifyUserAsync(userId))
            .Should().ThrowAsync<InvalidOperationException>();
    }
}