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
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        var expectedUser = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };

        // Configure mock to return the user
        mockRepo.GetByIdAsync(1).Returns(expectedUser);

        // Act
        var service = new NotificationService(mockRepo, mockEmail);
        await service.NotifyUserAsync(1);

        // Assert
        // 1. Verify email was sent with the correct address
        await mockEmail.Received(1).SendWelcomeAsync("alice@example.com");

        // 2. Verify repository was called
        await mockRepo.Received(1).GetByIdAsync(1);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        // Configure mock to return null (user not found)
        mockRepo.GetByIdAsync(99).Returns((User?)null);

        var service = new NotificationService(mockRepo, mockEmail);

        // Act & Assert
        await service.NotifyUserAsync(99);

        // Verify that the correct exception is thrown
        await Task.CompletedTask.Should().ThrowAsync<InvalidOperationException>();

        // Verify that the email service was NOT called
        await mockEmail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}