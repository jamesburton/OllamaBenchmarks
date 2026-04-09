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
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };

        repo.GetByIdAsync(user.Id).Returns(user);

        var sut = new NotificationService(repo, email);

        // Act
        await sut.NotifyUserAsync(user.Id);

        // Assert
        repo.Received(1).GetByIdAsync(user.Id);
        email.Received(1).SendWelcomeAsync(user.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        int missingUserId = 42;

        repo.GetByIdAsync(missingUserId).Returns((User?)null);

        var sut = new NotificationService(repo, email);

        // Act
        Func<Task> act = () => sut.NotifyUserAsync(missingUserId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        repo.Received(1).GetByIdAsync(missingUserId);
        email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}