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

[Fact]
public void NotifyUserAsync_WhenUserFound_SendsCorrectEmail()
{
    // Arrange
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();
    var service = new NotificationService(mockRepo, mockEmail);

    var testUser = new User { Id = 1, Email = "test@example.com" };
    mockRepo.GetByIdAsync(1).Returns(testUser);

    // Act
    service.NotifyUserAsync(1);

    // Assert
    mockEmail.Received().SendWelcomeAsync("test@example.com");
}

[Fact]
public void NotifyUserAsync_WhenUserNotFound_ThrowsException()
{
    // Arrange
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();
    var service = new NotificationService(mockRepo, mockEmail);

    mockRepo.GetByIdAsync(1).Returns((User?)null);

    // Act & Assert
    Func<Task> act = () => service.NotifyUserAsync(1);
    act.Should().ThrowAsync<InvalidOperationException>();

    // Verify no email was sent
    mockEmail.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
}