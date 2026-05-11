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
    public async Task TestNotifyUserAsync_HappyPath()
    {
        // Arrange
        var user = new User { Id = 123, Name = "John Doe", Email = "test@example.com" };
        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(123).Returns(Task.FromResult(user));
        var email = Substitute.For<IEmailService>();

        var service = new NotificationService(repo, email);

        // Act
        await service.NotifyUserAsync(123);

        // Assert
        await email.Received().SendWelcomeAsync("test@example.com");
        repo.Received().GetByIdAsync(123);
    }

    [Fact]
    public async Task TestNotifyUserAsync_UserNotFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(456).Returns(Task.FromResult<User?>(null));
        var email = Substitute.For<IEmailService>();

        var service = new NotificationService(repo, email);

        // Act & Assert
        var asyncAct = () => service.NotifyUserAsync(456);
        await asyncAct.Should().ThrowAsync<InvalidOperationException>();

        repo.Received().GetByIdAsync(456);
        email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}