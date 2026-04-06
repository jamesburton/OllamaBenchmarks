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
        var userId = 42;
        var user = new User { Id = userId, Name = "Ada", Email = "ada@example.com" };
        repo.GetByIdAsync(userId).Returns(user);

        var service = new NotificationService(repo, email);

        // Act
        await service.NotifyUserAsync(userId);

        // Assert
        await repo.Received().GetByIdAsync(userId);
        await email.Received().SendWelcomeAsync(user.Email);
        user.Email.Should().Be("ada@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var userId = 99;
        repo.GetByIdAsync(userId).Returns((User?)null);

        var service = new NotificationService(repo, email);

        // Act
        Func<Task> act = async () => await service.NotifyUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}