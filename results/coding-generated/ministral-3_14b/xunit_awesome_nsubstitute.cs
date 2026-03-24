using Xunit;
using NSubstitute;
using AwesomeAssertions;
using System;
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
    private readonly IUserRepository _repo = Substitute.For<IUserRepository>();
    private readonly IEmailService _email = Substitute.For<IEmailService>();
    private readonly NotificationService _service;

    public NotificationServiceTests()
    {
        _service = new NotificationService(_repo, _email);
    }

    [Fact]
    public async Task NotifyUserAsync_WhenUserFound_SendsWelcomeEmail()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com" };
        _repo.GetByIdAsync(1).Returns(user);

        // Act
        await _service.NotifyUserAsync(1);

        // Assert
        await _email.Received(1).SendWelcomeAsync("test@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        _repo.GetByIdAsync(1).Returns((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.NotifyUserAsync(1))
            .Should().HaveMessage("User not found");
    }

    [Fact]
    public async Task NotifyUserAsync_VerifiesCorrectMethodCalls()
    {
        // Arrange
        var user = new User { Id = 1, Email = "test@example.com" };
        _repo.GetByIdAsync(1).Returns(user);

        // Act
        await _service.NotifyUserAsync(1);

        // Assert
        await _repo.Received(1).GetByIdAsync(1);
        await _email.Received(1).SendWelcomeAsync(Arg.Is<string>(email => email == "test@example.com"));
    }
}