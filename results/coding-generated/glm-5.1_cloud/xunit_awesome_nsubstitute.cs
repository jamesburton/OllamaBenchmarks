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
    private readonly IEmailService _emailService;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _repo = Substitute.For<IUserRepository>();
        _emailService = Substitute.For<IEmailService>();
        _sut = new NotificationService(_repo, _emailService);
    }

    [Fact]
    public async Task NotifyUserAsync_WhenUserFound_SendsWelcomeEmailToCorrectAddress()
    {
        var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
        _repo.GetByIdAsync(1).Returns(user);

        await _sut.NotifyUserAsync(1);

        await _repo.Received(1).GetByIdAsync(1);
        await _emailService.Received(1).SendWelcomeAsync("alice@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        _repo.GetByIdAsync(99).Returns((User?)null);

        Func<Task> act = () => _sut.NotifyUserAsync(99);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await _emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}