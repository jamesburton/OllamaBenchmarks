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

using NSubstitute;
using AwesomeAssertions;
using Xunit;

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUserAsync_UserFound_SendsWelcomeEmailWithCorrectEmail()
    {
        // Arrange
        var userId = 123;
        var expectedEmail = "user@example.com";

        var user = new User { Id = userId, Email = expectedEmail };

        var emailService = Substitute.For<IEmailService>();
        emailService.SendWelcomeAsync(Arg.AsAny()).Returns(Task.CompletedTask);

        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(userId).ReturnsAsync(user);

        var notificationService = new NotificationService(repo, emailService);

        // Act
        await notificationService.NotifyUserAsync(userId);

        // Assert
        emailService.Received().SendWelcomeAsync(expectedEmail);
        notificationService.Received();
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 123;

        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(userId).ReturnsAsync((User?)null);

        var emailService = Substitute.For<IEmailService>();

        var notificationService = new NotificationService(repo, emailService);

        // Act & Assert
        await notificationService.NotifyUserAsync(userId)
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found");
    }

    [Theory]
    [MemberData(nameof(UserData))]
    public async Task NotifyUserAsync_UserNotFound_ThrowsInvalidOperationException(int userId)
    {
        // Arrange
        var expectedEmail = "user@example.com"; // irrelevant but passed

        var emailService = Substitute.For<IEmailService>();
        emailService.SendWelcomeAsync(Arg.AsAny()).Returns(Task.CompletedTask);

        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(userId).ReturnsAsync((User?)null);

        var notificationService = new NotificationService(repo, emailService);

        // Act & Assert
        await notificationService.NotifyUserAsync(userId)
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found");
    }

    public static object[] UserData =>
    {
        return new[]
        {
            new User { Id = 123, Email = "user@example.com" },
            new User { Id = 456, Email = "test@example.com" }
        };
    }

    [Theory]
    [MemberData(nameof(UserData))]
    public async Task NotifyUserAsync_SendsWelcomeEmailForEachUser(int userId, string expectedEmail)
    {
        // Arrange
        var user = new User { Id = userId, Email = expectedEmail };

        var emailService = Substitute.For<IEmailService>();
        emailService.SendWelcomeAsync(Arg.AsAny()).Returns(Task.CompletedTask);

        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(userId).ReturnsAsync(user);

        var notificationService = new NotificationService(repo, emailService);

        // Act
        await notificationService.NotifyUserAsync(userId);

        // Assert
        emailService.Received().SendWelcomeAsync(expectedEmail);
        notificationService.Received();
    }

    [Fact]
    public async Task NotifyUserAsync_VerifiesReceivedCalls()
    {
        // Arrange
        var userId = 789;
        var expectedEmail = "user@example.com";

        var emailService = Substitute.For<IEmailService>();
        emailService.SendWelcomeAsync(Arg.AsAny()).Returns(Task.CompletedTask);

        var repo = Substitute.For<IUserRepository>();
        repo.GetByIdAsync(userId).ReturnsAsync(new User { Id = userId, Email = expectedEmail });

        var notificationService = new NotificationService(repo, emailService);

        // Act
        await notificationService.NotifyUserAsync(userId);

        // Assert
        notificationService.Received();
        notificationService.Received().SendWelcomeAsync(expectedEmail);
    }
}