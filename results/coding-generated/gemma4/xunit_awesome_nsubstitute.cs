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
        var testUser = new User { Id = 1, Name = "Test User", Email = "test@example.com" };

        mockRepo.GetByIdAsync(1).Returns(Task.FromResult<User?>(testUser));

        var service = new NotificationService(mockRepo, mockEmail);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        // Verify calls
        await mockRepo.Received().GetByIdAsync(1);
        await mockEmail.Received().SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        // Simulate user not found (returns null)
        mockRepo.GetByIdAsync(99).Returns(Task.FromResult<User?>(null));

        var service = new NotificationService(mockRepo, mockEmail);

        // Act & Assert
        var act = async () => await service.NotifyUserAsync(99);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("User not found");

        // Verify that email service was never called
        await mockEmail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}