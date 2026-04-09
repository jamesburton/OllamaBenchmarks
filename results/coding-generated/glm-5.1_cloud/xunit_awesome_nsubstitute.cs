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
    public async Task NotifyUserAsync_UserFound_SendsWelcomeEmailToCorrectAddress()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        _repo.GetByIdAsync(user.Id).Returns(user);

        // Act
        await _sut.NotifyUserAsync(user.Id);

        // Assert
        await _repo.Received(1).GetByIdAsync(user.Id);
        await _email.Received(1).SendWelcomeAsync(user.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        int userId = 99;
        _repo.GetByIdAsync(userId).Returns((User?)null);

        // Act
        Func<Task> act = () => _sut.NotifyUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await _email.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}