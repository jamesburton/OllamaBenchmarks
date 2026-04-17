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
using Xunit;
using AwesomeAssertions;

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUserAsync_UserFound_SendsWelcomeEmail()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        var expectedUser = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };

        // Setup mock to return the user
        mockRepo.GetByIdAsync(1).Returns(expectedUser);

        // Setup mock to verify the email is sent
        mockEmail.SendWelcomeAsync(expectedUser.Email).Returns(Task.CompletedTask);

        var service = new NotificationService(mockRepo, mockEmail);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        // Verify repository interaction
        mockRepo.Received(1).GetByIdAsync(1);

        // Verify email service interaction
        mockEmail.Received(1).SendWelcomeAsync(expectedUser.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        // Setup mock to return null (user not found)
        mockRepo.GetByIdAsync(999).Returns((User?)null);

        var service = new NotificationService(mockRepo, mockEmail);

        // Act & Assert
        await service.NotifyUserAsync(999);

        // Verify that the expected exception is thrown
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.NotifyUserAsync(999);
        });

        // Verify that the email service was never called
        mockEmail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}