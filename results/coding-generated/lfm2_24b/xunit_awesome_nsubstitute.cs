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

public class NotificationService
{
    public async Task NotifyUserAsync(int userId)
    {
        var user = await repo.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found");
        await email.SendWelcomeAsync(user.Email);
    }

    public IUserRepository repo { get; }
    public IEmailService email { get; }

    public NotificationService(IUserRepository repo, IEmailService email)
    {
        this.repo = repo;
        this.email = email;
    }
}

public class NotificationServiceTests
{
    public void TestHappyPath()
    {
        var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        mockRepo.GetByIdAsync(1).ReturnsAsync(user);
        mockEmail.SendWelcomeAsync(Arg.Is("alice@example.com"));

        var service = new NotificationService(mockRepo, mockEmail);
        service.NotifyUserAsync(1).Wait();

        mockRepo.Received(1).GetByIdAsync(1);
        mockEmail.Received(1).SendWelcomeAsync("alice@example.com");
    }

    public void TestUserNotFound()
    {
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        mockRepo.GetByIdAsync(999).ReturnsAsync((User?)null);

        var service = new NotificationService(mockRepo, mockEmail);
        var act = () => service.NotifyUserAsync(999);

        act.Should().ThrowAsync<InvalidOperationException>()
           .WithMessage("User not found");

        mockRepo.Received(1).GetByIdAsync(999);
        mockEmail.DidNotReceive();
    }
}