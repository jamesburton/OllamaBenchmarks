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
        var testUser = new User(Id: 1, Name: "Alice", Email: "alice@example.com");

        // Setup mocks for the happy path
        mockRepo.GetByIdAsync(1).Returns(Task.FromResult(testUser));

        var service = new NotificationService(mockRepo, mockEmail);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        // 1. Verify the repository was called correctly
        await mockRepo.Received(1).GetByIdAsync(1);

        // 2. Verify the email service was called with the correct data
        await mockEmail.Received(1).SendWelcomeAsync("alice@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        // Setup mock to return null (user not found)
        mockRepo.GetByIdAsync(99).Returns(Task.FromResult<User?>(null));

        var service = new NotificationService(mockRepo, mockEmail);

        // Act & Assert
        // Use AwesomeAssertions for exception testing
        await () => service.NotifyUserAsync(99)
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found");

        // Ensure no side effects occurred
        await mockEmail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}