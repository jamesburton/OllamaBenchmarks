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
    public async Task NotifyUser_WhenUserFound_SendsEmail()
    {
        // Arrange
        var expectedEmail = "test@example.com";
        var user = new User { Id = 1, Email = expectedEmail };

        var repoMock = Substitute.For<IUserRepository>();
        var emailMock = Substitute.For<IEmailService>();

        repoMock.GetByIdAsync(1).Returns(user);
        emailMock.SendWelcomeAsync(expectedEmail);

        // Act
        await NotificationService(repoMock, emailMock).NotifyUserAsync(1);

        // Assert
        emailMock.Received().SendWelcomeAsync(expectedEmail);
    }

    [Fact]
    public async Task NotifyUser_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var repoMock = Substitute.For<IUserRepository>();
        var emailMock = Substitute.For<IEmailService>();

        repoMock.GetByIdAsync(1).ReturnsNull();

        // Act & Assert
        await repoMock.GetByIdAsync(1)
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found");
    }

    [Fact]
    public async Task NotifyUser_WhenRepoThrowsException_PassesThrough()
    {
        // Arrange
        var expectedEmail = "test@example.com";
        var user = new User { Id = 1, Email = expectedEmail };

        var repoMock = Substitute.For<IUserRepository>();
        var emailMock = Substitute.For<IEmailService>();

        repoMock.GetByIdAsync(1).Returns(user);
        emailMock.SendWelcomeAsync(Arg.Any<string>());

        // Act & Assert
        await NotificationService(repoMock, emailMock).NotifyUserAsync(1);

        emailMock.Received().SendWelcomeAsync(expectedEmail);
    }
}