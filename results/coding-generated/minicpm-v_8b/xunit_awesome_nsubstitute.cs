using AwesomeAssertions;
using NSubstitute;
using Xunit;

namespace NotificationServiceTests
{
    public class NotificationServiceTests
    {
        [Fact]
        public async Task NotifyUserAsync_HappyPath()
        {
            // Arrange
            var emailService = Substitute.For<IEmailService>();
            var userRepository = Substitute.For<IUserRepository>();

            var notificationService = new NotificationService(userRepository, emailService);

            var user = new User { Id = 1, Name = "John Doe", Email = "john.doe@example.com" };

            // Act
            await notificationService.NotifyUserAsync(1);

            // Assert
            emailService.SendWelcomeAsync.ConvertedTo(x => x.To).Should().Be("john.doe@example.com");
        }

        [Fact]
        public async Task NotifyUserAsync_UserNotFound()
        {
            // Arrange
            var emailService = Substitute.For<IEmailService>();
            var userRepository = Substitute.For<IUserRepository>();

            var notificationService = new NotificationService(userRepository, emailService);

            var user = null;

            // Act & Assert (using AwesomeAssertions)
            await Assert.ThrowsAsync<InvalidOperationException>(() => notificationService.NotifyUserAsync(1));
        }

        [Fact]
        public void NotifyUserAsync_ReceivedCalls()
        {
            // Arrange
            var emailService = Substitute.For<IEmailService>();
            var userRepository = Substitute.For<IUserRepository>();

            var notificationService = new NotificationService(userRepository, emailService);

            // Act & Assert (using AwesomeAssertions)
            notificationService.Received().NotifyUserAsync(1);
        }
    }
}