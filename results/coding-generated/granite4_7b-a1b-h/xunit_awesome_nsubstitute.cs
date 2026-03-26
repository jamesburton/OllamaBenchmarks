using Xunit;
using NSubstitute;
using AwesomeAssertions;

public class NotificationServiceTests
{
    [Fact]
    public async Task NotifyUserAsync_HappyPath()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        userRepository.GetByIdAsync(1).Returns(new User { Id = 1, Email = "user@example.com" });

        var notificationService = new NotificationService(userRepository, emailService);

        // Act
        await notificationService.NotifyUserAsync(1);

        // Assert
        userRepository.Received().GetByIdAsync(1);
        emailService.Received().SendWelcomeAsync("user@example.com");
    }

    [Fact]
    public async Task NotifyUserAsync_UserNotFound()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        userRepository.GetByIdAsync(2).Returns(null);

        var notificationService = new NotificationService(userRepository, emailService);
        Action act = () => notificationService.NotifyUserAsync(2);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*not found*");
    }

    [Fact]
    public void NotifyUserAsync_ReceivedCalls()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        var emailService = Substitute.For<IEmailService>();

        var notificationService = new NotificationService(userRepository, emailService);

        // Act
        notificationService.NotifyUserAsync(3);

        // Assert
        userRepository.Received().GetByIdAsync(3);
        emailService.Received().SendWelcomeAsync(Arg.Any<string>());
    }
}