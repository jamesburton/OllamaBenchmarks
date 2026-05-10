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
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _repo = Substitute.For<IUserRepository>();
        _email = Substitute.For<IEmailService>();
        _service = new NotificationService(_repo, _email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserFound_SendsEmail()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test", Email = "test@example.com" };
        _repo.GetByIdAsync(1).Returns(user);

        // Act
        await _service.NotifyUserAsync(1);

        // Assert
        _email.Received().SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _repo.GetByIdAsync(999).Returns((User?)null);

        // Act
        Func<Task> act = () => _service.NotifyUserAsync(999);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}