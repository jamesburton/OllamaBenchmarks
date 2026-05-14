using System.Threading.Tasks;

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
    public async Task NotifyUserAsync_UserFound_SendsWelcomeEmailWithCorrectEmail()
    {
        // Arrange
        var expectedUser = new User { Id = 1, Email = "test@example.com" };
        _repo.GetByIdAsync(1).Returns(Task.FromResult<User?>(expectedUser));

        // Act
        await _sut.NotifyUserAsync(1);

        // Assert
        _email.Received(1).SendWelcomeAsync(expectedUser.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _repo.GetByIdAsync(1).Returns(Task.FromResult<User?>(null));

        // Act
        Func<Task> act = async () => await _sut.NotifyUserAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task NotifyUserAsync_UserFound_CallsGetByIdAsyncOnce()
    {
        // Arrange
        _repo.GetByIdAsync(1).Returns(Task.FromResult<User?>(null));

        // Act
        await _sut.NotifyUserAsync(1);

        // Assert
        _repo.Received(1).GetByIdAsync(1);
    }

    [Fact]
    public async Task NotifyUserAsync_UserFound_CallsSendWelcomeAsyncOnce()
    {
        // Arrange
        _repo.GetByIdAsync(1).Returns(Task.FromResult<User?>(new User { Email = "user@example.com" }));

        // Act
        await _sut.NotifyUserAsync(1);

        // Assert
        _email.Received(1).SendWelcomeAsync("user@example.com");
    }
}