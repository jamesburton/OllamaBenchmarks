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
public async Task HappyPath_UserFound_EmailSent()
{
    // Arrange
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
    mockRepo.GetByIdAsync(1).Returns(user);

    var service = new NotificationService(mockRepo, mockEmail);

    // Act
    await service.NotifyUserAsync(1);

    // Assert
    mockRepo.Received().GetByIdAsync(1);
    mockEmail.Received().SendWelcomeAsync("john@example.com");
}

[Fact]
public async Task UserNotFound_ThrowsInvalidOperationException()
{
    // Arrange
    var mockRepo = Substitute.For<IUserRepository>();
    var mockEmail = Substitute.For<IEmailService>();

    mockRepo.GetByIdAsync(Arg.Any<int>()).Returns((User?)null);

    var service = new NotificationService(mockRepo, mockEmail);

    Func<Task> act = async () => await service.NotifyUserAsync(1);

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>();
}