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
    public async Task NotifyUserAsync_HappyPath_SendsWelcomeEmail()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var user = new User { Id = 1, Email = "test@example.com" };
        userRepository.GetByIdAsync(1).Returns(user);
        var notificationService = new NotificationService(userRepository, emailService);

        // Act
        await notificationService.NotifyUserAsync(1);

        // Assert
        await emailService.Received().SendWelcomeAsync("test@example.com");
        userRepository.Received().GetByIdAsync(1);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        userRepository.GetByIdAsync(1).Returns((User?)null);
        var notificationService = new NotificationService(userRepository, emailService);

        // Act
        Func<Task> act = async () => await notificationService.NotifyUserAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        userRepository.Received().GetByIdAsync(1);
        await emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}