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

using Xunit;
using NSubstitute;
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

        // Act
        var service = new NotificationService(mockRepo, mockEmail);
        await service.NotifyUserAsync(1);

        // Assert
        // Verify that the email service was called exactly once with the correct email
        await mockEmail.Received(1).SendWelcomeAsync("alice@example.com");

        // Optional: Verify repository interaction
        await mockRepo.Received(1).GetByIdAsync(1);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        // Setup mock to return null (user not found)
        mockRepo.GetByIdAsync(99).Returns((User?)null);

        var service = new NotificationService(mockRepo, mockEmail);

        // Act & Assert
        await mockEmail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());

        await service.NotifyUserAsync(99);

        // Assert that the expected exception is thrown
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.NotifyUserAsync(99));
    }
}