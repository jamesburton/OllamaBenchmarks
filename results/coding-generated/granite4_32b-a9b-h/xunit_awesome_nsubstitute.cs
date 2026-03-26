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
public void HappyPath()
{
    // Arrange
    var userId = 42;
    var userEmail = "test@example.com";
    User? user = null;

    IUserRepository repo = Substitute.For<IUserRepository>();
    IEmailService email = Substitute.For<IEmailService>();

    repo.GetByIdAsync(userId).Returns(user);
    email.SendWelcomeAsync(userEmail).Returns(Task.CompletedTask);

    // Act
    var sut = new NotificationService(repo, email);
    Func<Task> act = () => sut.NotifyUserAsync(userId);

    // Assert
    user.Should().NotBeNull();
    act.Should().NotThrow();
    email.Received().SendWelcomeAsync(userEmail);
}

[Fact]
public void UserNotFound()
{
    // Arrange
    var userId = 99;
    User? user = null;

    IUserRepository repo = Substitute.For<IUserRepository>();
    IEmailService email = Substitute.For<IEmailService>();

    repo.GetByIdAsync(userId).Returns(user);

    // Act
    var sut = new NotificationService(repo, email);
    Func<Task> act = () => sut.NotifyUserAsync(userId);

    // Assert
    act.Should().ThrowAsync<InvalidOperationException>();
}