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
    private IUserRepository CreateRepo() => Substitute.For<IUserRepository>();
    private IEmailService CreateEmail() => Substitute.For<IEmailService>();
    private NotificationService CreateService(IUserRepository repo, IEmailService email) => new NotificationService(repo, email);

    [Fact]
    public async Task NotifyUserAsync_WhenUserExists_SendsWelcomeEmail()
    {
        var repo = CreateRepo();
        var email = CreateEmail();
        var userId = 1;
        var user = new User { Id = userId, Name = "Bob", Email = "bob@example.com" };
        repo.GetByIdAsync(userId).Returns(user);
        var service = CreateService(repo, email);

        await service.NotifyUserAsync(userId);

        await email.Received().SendWelcomeAsync(user.Email);
        repo.Received().GetByIdAsync(userId);
    }

    [Fact]
    public