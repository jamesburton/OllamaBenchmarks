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
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _emailService = Substitute.For<IEmailService>();
        _sut = new NotificationService(_userRepository, _emailService);
    }

    [Fact]
    public async Task NotifyUserAsync_ShouldSendWelcomeEmail_WhenUserIsFound()
    {
        // Arrange
        var user = new User { Id = 1, Name = "Test User", Email = "test@example.com" };
        _userRepository.GetByIdAsync(user.Id).Returns(user);

        // Act
        await _sut.NotifyUserAsync(user.Id);

        // Assert
        await _emailService.Received().SendWelcomeAsync(user.Email);
    }

    [Fact]
    public async Task NotifyUserAsync_ShouldThrowInvalidOperationException_WhenUserIsNotFound()
    {
        // Arrange
        int userId = 999;
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        var act = async () => await _sut.NotifyUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        await _emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}