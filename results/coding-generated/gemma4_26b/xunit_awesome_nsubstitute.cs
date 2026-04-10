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
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly NotificationService _sut = new(_userRepository, _emailService);

    [Fact]
    public async Task NotifyUserAsync_WhenUserExists_SendsEmailWithCorrectAddress()
    {
        // Arrange
        var userId = 1;
        var expectedUser = new User { Id = userId, Name = "John Doe", Email = "john@example.com" };
        _userRepository.GetByIdAsync(userId).Returns(expectedUser);

        // Act
        await _sut.NotifyUserAsync(userId);

        // Assert
        _emailService.Received(1).SendWelcomeAsync("john@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_WhenUserDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 99;
        _userRepository.GetByIdAsync(userId).Returns((User?)null);

        // Act
        Func<Task> act = () => _sut.NotifyUserAsync(userId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        _emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }
}