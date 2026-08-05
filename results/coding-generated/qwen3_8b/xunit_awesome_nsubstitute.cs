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
    public async Task NotifyUserAsync_HappyPath_UserFoundAndEmailSent()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);

        var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
        repo.GetByIdAsync(1).Returns(user);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        await email.Received(1).SendWelcomeAsync("alice@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);

        repo.GetByIdAsync(1).Returns((User?)null);

        // Act & Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task NotifyUserAsync_UserFound_EmailSent()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);

        var user = new User { Id = 1, Name = "Bob", Email = "bob@example.com" };
        repo.GetByIdAsync(1).Returns(user);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        await email.Received(1).SendWelcomeAsync("bob@example.com");
    }
}