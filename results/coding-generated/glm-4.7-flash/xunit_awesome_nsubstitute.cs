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
    public async Task NotifyUserAsync_HappyPath_UserFound_EmailSent()
    {
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        var user = new User { Id = 1, Name = "Test", Email = "test@example.com" };
        repo.GetByIdAsync(1).Returns(user);

        var service = new NotificationService(repo, email);

        await service.NotifyUserAsync(1);

        email.Received().SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(1).Returns((User?)null);

        var service = new NotificationService(repo, email);

        await service.NotifyUserAsync(1);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_VerifyNoEmailSent()
    {
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(1).Returns((User?)null);

        var service = new NotificationService(repo, email);

        await service.NotifyUserAsync(1);

        email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}