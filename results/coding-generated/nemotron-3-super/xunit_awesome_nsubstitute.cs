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
    public async Task NotifyUserAsync_HappyPath_SendsEmail()
    {
        var repo = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, emailService);
        int userId = 42;
        var user = new User { Id = userId, Name = "Alice", Email = "alice@example.com" };
        repo.GetByIdAsync(userId).Returns(user);

        await service.NotifyUserAsync(userId);

        await repo.Received(1).GetByIdAsync(userId);
        await emailService.Received(1).SendWelcomeAsync(user.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_Throws()
    {
        var repo = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, emailService);
        int userId = 99;
        repo.GetByIdAsync(userId).Returns(null as User?);

        Func<Task> act = () => service.NotifyUserAsync(userId);
        await act.Should().ThrowAsync<InvalidOperationException>();
        await repo.Received(1).GetByIdAsync(userId);
        await emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}