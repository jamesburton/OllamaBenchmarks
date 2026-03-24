using NSubstitute;
using AwesomeAssertions;
using Xunit;

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
    private readonly IUserRepository _repo;
    private readonly IEmailService _email;

    public NotificationService(IUserRepository repo, IEmailService email)
    {
        _repo = repo;
        _email = email;
    }

    public async Task NotifyUserAsync(int userId)
    {
        var user = await _repo.GetByIdAsync(userId)
            ?? throw new InvalidOperationException("User not found");
        await _email.SendWelcomeAsync(user.Email);
    }
}

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUserAsync_HappyPath_UserFoundAndEmailSent()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        var user = new User
        {
            Id = 1,
            Name = "John Doe",
            Email = "john@example.com"
        };

        mockRepo.GetByIdAsync(1).Returns(Task.FromResult(user));
        mockEmail.SendWelcomeAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

        var service = new NotificationService(mockRepo, mockEmail);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        await mockRepo.Received(1).GetByIdAsync(1);
        await mockEmail.Received(1).SendWelcomeAsync("john@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var mockRepo = Substitute.For<IUserRepository>();
        var mockEmail = Substitute.For<IEmailService>();

        mockRepo.GetByIdAsync(999).Returns(Task.FromResult<User?>(null));

        var service = new NotificationService(mockRepo, mockEmail);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            service.NotifyUserAsync(999));
    }
}