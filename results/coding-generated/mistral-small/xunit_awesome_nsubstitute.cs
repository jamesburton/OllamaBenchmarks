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
        var userRepo = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        var userId = 1;
        var user = new User { Id = userId, Email = "test@example.com" };
        userRepo.GetByIdAsync(userId).Returns(user);

        var notificationService = new NotificationService(userRepo, emailService);

        // Act
        await notificationService.NotifyUserAsync(userId);

        // Assert
        userRepo.Received(1).GetByIdAsync(userId);
        emailService.Received(1).SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound()
    {
        // Arrange
        var userRepo = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        var userId = 1;
        userRepo.GetByIdAsync(userId).Returns((User?)null);

        var notificationService = new NotificationService(userRepo, emailService);

        // Act
        Func<Task> act = async () => await notificationService.NotifyUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}