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
        var userId = 1;
        var user = new User { Id = userId, Email = "test@example.com" };

        var repo = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        repo.GetByIdAsync(userId).Returns(user);

        string? capturedEmail = null;
        emailService.When(x => x.SendWelcomeAsync(Arg.Any<string>()))
                    .Do(callInfo => capturedEmail = callInfo.Arg<string>());

        var sut = new NotificationService(repo, emailService);

        // Act
        await sut.NotifyUserAsync(userId);

        // Assert
        await repo.Received(1).GetByIdAsync(userId);
        capturedEmail.Should().Be(user.Email);
        await emailService.Received(1).SendWelcomeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var repo = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        repo.GetByIdAsync(userId).Returns((User?)null);

        var sut = new NotificationService(repo, emailService);

        // Act
        Func<Task> act = () => sut.NotifyUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}