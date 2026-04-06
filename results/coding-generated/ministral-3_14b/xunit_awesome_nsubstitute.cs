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
    public async Task NotifyUserAsync_SendsWelcomeEmail_WhenUserFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);

        var testUser = new User { Id = 1, Email = "test@example.com" };
        repo.GetByIdAsync(1).Returns(testUser);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        await email.Received(1).SendWelcomeAsync(testUser.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_ThrowsInvalidOperationException_WhenUserNotFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);

        repo.GetByIdAsync(1).Returns((User?)null);

        // Act & Assert
        await service.NotifyUserAsync(1)
            .Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task NotifyUserAsync_VerifiesCorrectCalls_WhenUserFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);

        var testUser = new User { Id = 1, Email = "test@example.com" };
        repo.GetByIdAsync(1).Returns(testUser);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        repo.Received(1).GetByIdAsync(1);
        email.Received(1).SendWelcomeAsync(testUser.Email);
    }
}