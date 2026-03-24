using System;
using NSubstitute;
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
    public async Task NotifyUserAsync_HappyPath()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();

        repo.GetByIdAsync(1).Returns(new User { Id = 1, Email = "user@example.com" });

        var service = new NotificationService(repo, email);

        // Act
        await service.NotifyUserAsync(1);

        // Assert
        repo.Received().GetByIdAsync(1);
        email.Received().SendWelcomeAsync("user@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(2).Returns(null);

        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.NotifyUserAsync(2));
        repo.Received().GetByIdAsync(2);
    }

    [Fact]
    public void NotifyUserAsync_RepositoryReceivedCalls()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);

        // Act
        service.NotifyUserAsync(3);

        // Assert
        repo.Received().GetByIdAsync(3);
    }

    [Fact]
    public void NotifyUserAsync_EmailReceivedCalls()
    {
        // Arrange
        var repo = Substitute.For<IUserRepository>();
        var email = Substitute.For<IEmailService>();
        var service = new NotificationService(repo, email);

        // Act
        service.NotifyUserAsync(4);

        // Assert
        email.Received().SendWelcomeAsync(Arg.Any<string>());
    }
}