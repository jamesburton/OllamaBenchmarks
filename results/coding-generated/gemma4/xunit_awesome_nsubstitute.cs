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

        // Setup successful retrieval
        mockRepo.GetByIdAsync(1).Returns(Task.FromResult(testUser));

        var service = new NotificationService(mockRepo, mockEmail);

        // Act
        await service.NotifyUserAsync(1);

        // Assert (Using AwesomeAssertions for verification)
        // We verify the email service was called with the correct email
        await mockEmail.Received(1).SendWelcomeAsync("alice@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        // Setup retrieval failure (returns null)
        mockRepo.GetByIdAsync(99).Returns(Task.FromResult<User?>(null));

        var service = new NotificationService(mockRepo, mockEmail);

        // Act & Assert (Using AwesomeAssertions for exception handling)
        var act = async () => await service.NotifyUserAsync(99);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found");

        // Verify that the email service was never called
        await mockEmail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}