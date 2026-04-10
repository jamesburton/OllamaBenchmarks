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
        userRepository.GetByIdAsync(2).Returns(null);

        var emailService = Substitute.For<IEmailService>();

        var notificationService = new NotificationService(userRepository, emailService);

        // Act & Assert
        await notificationService.NotifyUserAsync(2).Should().ThrowAsync<InvalidOperationException>();
        userRepository.Received().GetByIdAsync(2);
        emailService.DidNotReceive().SendWelcomeAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task NotifyUserAsync_ThrowsCorrectException()
    {
        // Arrange
        var userRepository = Substitute.For<IUserRepository>();
        userRepository.GetByIdAsync(3).Returns(null);

        var emailService = Substitute.For<IEmailService>();

        var notificationService = new NotificationService(userRepository, emailService);

        // Act
        var act = notificationService.NotifyUserAsync(3);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*not found*");
    }
}