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
    private Substitute.IContainer _c;

    public NotificationServiceTests()
    {
        _c = Substitute.NewContainer()
            .For<IUserRepository>(Substitute.For<IUserRepository>())
            .For<IEmailService>(Substitute.For<IEmailService>());
    }

    [Fact]
    public async Task HappyPath_SendEmailWhenUserExists()
    {
        var repo = _c.For<IUserRepository>();
        repo.GetByIdAsync(1).Returns(new User { Id = 1, Name = "Alice", Email = "alice@example.com" });

        var email = _c.For<IEmailService>();
        email.SendWelcomeAsync("alice@example.com").Returns();

        var svc = new NotificationService(repo, email);
        await svc.NotifyUserAsync(1).Should().NotThrow();

        Expect(repo).Received()
            .Call<NotificationService>((c, arg) => arg.Arg<int>() == 1, (c, a) => a);
        Expect(email).ReturnedOnce((sub, args) => args.Arg<string>() == "alice@example.com");
    }

    [Fact]
    public async Task NotFound_ThrowsInvalidOperationException()
    {
        var repo = _c.For<IUserRepository>();
        repo.GetByIdAsync(99).Returns((User?)null);

        var email = _c.For<IEmailService>();
        var svc = new NotificationService(repo, email);

        await svc.NotifyUserAsync(99).Should().ThrowAsync<InvalidOperationException>();

        repo.Received().GetByIdAsync(99);
    }
}