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
    public async Task NotifyUserAsync_HappyPath_SendsEmailWithCorrectAddress()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        var expectedUser = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        mockRepo.GetByIdAsync(1).Returns(expectedUser);

        var service = new NotificationService(mockRepo, mockEmail);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        await mockEmail.Received(1).SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        mockRepo.GetByIdAsync(1).Returns((User?)null);

        var service = new NotificationService(mockRepo, mockEmail);

        // Act & Assert
        Func<Task> act = () => service.NotifyUserAsync(1);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Verify email was NOT sent
        await mockEmail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}