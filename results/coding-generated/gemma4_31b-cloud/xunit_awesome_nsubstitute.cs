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
    public async Task NotifyUserAsync_UserExists_SendsWelcomeEmail()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var sut = new NotificationService(repo, emailService);

        var userId = 1;
        var user = new User { Id = userId, Email = "test@example.com", Name = "Test User" };

        repo.GetByIdAsync(userId).Returns(user);

        // Act
        await sut.NotifyUserAsync(userId);

        // Assert
        await repo.Received(1).GetByIdAsync(userId);
        await emailService.Received(1).SendWelcomeAsync(user.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var sut = new NotificationService(repo, emailService);

        int userId = 99;
        repo.GetByIdAsync(userId).Returns((User?)null);

        // Act
        Func<Task> act = () => sut.NotifyUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}