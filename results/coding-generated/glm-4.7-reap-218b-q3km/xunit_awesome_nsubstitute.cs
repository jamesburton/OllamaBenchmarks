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
    public async Task NotifyUserAsync_ShouldSendEmail_WhenUserIsFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var sut = new NotificationService(repo, email);

        var testUser = new User { Id = 1, Email = "test@example.com" };
        repo.GetByIdAsync(1).Returns(testUser);

        // Act
        await sut.NotifyUserAsync(1);

        // Assert
        repo.Received().GetByIdAsync(1);
        email.Received().SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var sut = new NotificationService(repo, email);

        repo.GetByIdAsync(1).Returns(null);

        // Act
        Func<Task> act = () => sut.NotifyUserAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}