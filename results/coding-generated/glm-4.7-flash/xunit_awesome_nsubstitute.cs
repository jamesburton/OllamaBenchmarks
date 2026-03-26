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
        var user = new User { Id = 1, Email = "test@example.com" };
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        repo.GetByIdAsync(1).Returns(user);

        var sut = new NotificationService(repo, email);

        // Act
        await sut.NotifyUserAsync(1);

        // Assert
        email.Received().SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(1).Returns((User?)null);

        var sut = new NotificationService(repo, Substitute.For<IEmailService>());

        // Act
        Func<Task> act = () => sut.NotifyUserAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}