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
    private readonly IUserRepository _repo;
    private readonly IEmailService _email;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _repo = Substitute.For<IUserRepository>();
        _email = Substitute.For<IEmailService>();
        _sut = new NotificationService(_repo, _email);
    }

    [Fact]
    public async Task NotifyUserAsync_HappyPath()
    {
        var user = new User { Id = 1, Name = "John", Email = "john@example.com" };
        _repo.GetByIdAsync(1).Returns(Task.FromResult(user));
        _email.SendWelcomeAsync("john@example.com").Returns(Task.CompletedTask);

        await _sut.NotifyUserAsync(1);

        _repo.Received().GetByIdAsync(1);
        _email.Received().SendWelcomeAsync("john@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_Throws()
    {
        _repo.GetByIdAsync(999).Returns(Task.FromResult<User?>(null));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.NotifyUserAsync(999));
    }

    [Fact]
    public async Task NotifyUserAsync_VerifyEmailSentWithCorrectAddress()
    {
        var user = new User { Id = 1, Name = "Jane", Email = "jane@example.com" };
        _repo.GetByIdAsync(1).Returns(Task.FromResult(user));

        await _sut.NotifyUserAsync(1);

        _email.Received().SendWelcomeAsync("jane@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_VerifyRepositoryCalledOnce()
    {
        var user = new User { Id = 1, Name = "Bob", Email = "bob@example.com" };
        _repo.GetByIdAsync(1).Returns(Task.FromResult(user));
        _email.SendWelcomeAsync("bob@example.com").Returns(Task.CompletedTask);

        await _sut.NotifyUserAsync(1);

        _repo.Received(1).GetByIdAsync(1);
    }

    [Fact]
    public async Task NotifyUserAsync_VerifyEmailCalledOnce()
    {
        var user = new User { Id = 2, Name = "Alice", Email = "alice@example.com" };
        _repo.GetByIdAsync(2).Returns(Task.FromResult(user));
        _email.SendWelcomeAsync("alice@example.com").Returns(Task.CompletedTask);

        await _sut.NotifyUserAsync(2);

        _email.Received(1).SendWelcomeAsync("alice@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_RepositoryCalledWithCorrectUserId()
    {
        var user = new User { Id = 42, Name = "Charlie", Email = "charlie@example.com" };
        _repo.GetByIdAsync(42).Returns(Task.FromResult(user));

        await _sut.NotifyUserAsync(42);

        _repo.Received().GetByIdAsync(42);
    }
}