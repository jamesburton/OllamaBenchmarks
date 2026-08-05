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
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IEmailService _email = Substitute.For<IEmailService>();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(_repo, _email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserFound_SendsWelcomeEmail()
    {
        var user = new User { Id = 1, Email = "test@example.com" };
        _repo.GetByIdAsync(1).Returns(user);

        await _sut.NotifyUserAsync(1);

        await _email.Received().SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        _repo.GetByIdAsync(99).Returns((User?)null);

        Func<Task> act = () => _sut.NotifyUserAsync(99);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}