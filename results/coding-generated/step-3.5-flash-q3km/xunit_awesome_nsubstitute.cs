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
        var userId = 42;
        var user = new User { Id = userId, Name = "Test", Email = "test@example.com" };
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(userId).Returns(user);

        var service = new NotificationService(repo, email);

        await service.NotifyUserAsync(userId);

        await repo.Received().GetByIdAsync(userId);
        await email.Received().SendWelcomeAsync(user.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        var userId = 42;
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(userId).Returns((User?)null);

        var service = new NotificationService(repo, email);

        Func<Task> act = () => service.NotifyUserAsync(userId);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await repo.Received().GetByIdAsync(userId);
        await email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}